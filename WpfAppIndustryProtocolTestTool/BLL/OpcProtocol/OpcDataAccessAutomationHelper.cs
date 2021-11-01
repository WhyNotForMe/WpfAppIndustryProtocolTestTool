using OPCAutomation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using WpfAppIndustryProtocolTestTool.Model;

namespace WpfAppIndustryProtocolTestTool.BLL.OpcProtocol
{
    public class OpcDataAccessAutomationHelper
    {

        #region OPC DA Automation Properties

        OPCServer _opcServer;
        OPCBrowser _opcBrowser;
        OPCGroups _opcGroups;
        OPCGroup _opcGroup;
        OPCItems _opcItems;
        OPCItem _opcItem;

        #endregion

        public bool IsConnected { get => _opcServer.ServerState == 1; }
        public List<OpcTagItemModel> OpcTagItemList { get; set; }
        public string ItemChanged { get; set; }
        public event OnItemValueChange ItemValueChanged;
        public event OnConnectionChange ConnectionChanged;

        //AddItem (parameters)
        string _itemID;
        int _clientHandle;
        Array _serverHandles;


        //OPC tags buffer
        List<OPCItem> _opcItemList;
        List<int> _serverHandleList;
        List<object> _writeValueList;


        //AsyncRead (parameters)
        int _readTransactionID;


        //AsyncWrite
        Array _writeValues;
        int _writeTransactionID;




        public OpcDataAccessAutomationHelper()
        {
            _opcServer = new OPCServer();

            OpcTagItemList = new List<OpcTagItemModel>();

            _opcItemList = new List<OPCItem>();

            _serverHandleList = new List<int>();

            _writeValueList = new List<object>();
        }

        public void GetHostNode(Collection<string> HostCollection)
        {
            try
            {
                //An System.Net.IPHostEntry instance that contains address information about 
                //the host specified in hostNameOrAddress.
                IPHostEntry iPHostNode = Dns.GetHostEntry(Environment.MachineName);
                var ipAddressList = iPHostNode.AddressList.ToList().FindAll(i => i.AddressFamily == AddressFamily.InterNetwork);
                foreach (var ipAddress in ipAddressList)
                {
                    if (!HostCollection.Contains(ipAddress.ToString()))
                    {
                        HostCollection.Add(ipAddress.ToString());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        public void GetServerName(Collection<string> ServerCollection, string SelectedHost)
        {
            try
            {

                object ServerList = _opcServer.GetOPCServers(SelectedHost);
                foreach (string serverName in (Array)ServerList)
                {
                    if (!ServerCollection.Contains(serverName))
                    {
                        ServerCollection.Add(serverName);
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// ConnectOPCServer
        /// </summary>
        /// <param name="serverName"> Server Name </param>
        /// <param name="tagCollection"> TagTree </param>
        /// <param name="groupName"></param>
        /// <param name="hostNode"></param>
        public void ConnectServer(string serverName, Collection<OpcTagTreeNodeModel> tagCollection, OpcGroupPropertyModel groupProperty)
        {
            try
            {
                _opcServer.Connect(serverName);
                _opcBrowser = _opcServer.CreateBrowser();
                LoadTagToTree(tagCollection);

                OpcTagItemList.Clear();

                _opcGroups = _opcServer.OPCGroups;
                _opcGroups.DefaultGroupDeadband = groupProperty.DefaultGroupDeadband;

                _opcGroup = _opcGroups.Add(groupProperty.GroupName);
                _opcGroup.IsActive = groupProperty.IsActive;
                _opcGroup.IsSubscribed = groupProperty.IsSubscribed;
                _opcGroup.UpdateRate = groupProperty.UpdateRate;
                _opcGroup.AsyncReadComplete += IKepGroup_AsyncReadComplete;


                _opcItems = _opcGroup.OPCItems;

                ConnectionChanged?.Invoke(serverName);
            }
            catch (Exception)
            {

                throw;
            }

        }


        private void LoadTagToTree(Collection<OpcTagTreeNodeModel> tagCollection)
        {
            _opcBrowser.ShowBranches();

            if (_opcBrowser.Count > 0)
            {
                foreach (var branch in _opcBrowser)
                {
                    OpcTagTreeNodeModel branchNode = new OpcTagTreeNodeModel { NodeName = branch.ToString() };
                    tagCollection.Add(branchNode);

                    _opcBrowser.MoveDown(branch.ToString());
                    LoadTagToTree(branchNode.Children);
                    _opcBrowser.MoveUp();
                }
            }

            _opcBrowser.ShowLeafs(false);
            if (_opcBrowser.Count > 0)
            {
                foreach (var leaf in _opcBrowser)
                {
                    OpcTagTreeNodeModel leafNode = new OpcTagTreeNodeModel { NodeName = leaf.ToString() };
                    leafNode.ItemID = _opcBrowser.GetItemID(leaf.ToString());
                    tagCollection.Add(leafNode);
                }
            }

        }
        /// <summary>
        /// DisconnectOPCServer
        /// </summary>
        public void DisconnectServer()
        {
            _opcServer.Disconnect();
            OpcTagItemList.Clear();
            ConnectionChanged?.Invoke(_opcServer.ServerName);

        }

        public string GetServerName()
        {
            return _opcServer.ServerName;
        }


        public string GetServerState()
        {
            switch (_opcServer.ServerState)
            {
                case 1:
                    return "Connected";
                case 6:
                    return "Disconnected";
                default:
                    return "Unknown";
            }
        }

        public void AddOpcTagItem(string SelectedTag)
        {
            try
            {

                if (OpcTagItemList.Count == 0)
                {
                    _serverHandleList.Clear();

                    _itemID = "0";
                    _clientHandle = 0;
                    _serverHandleList.Add(0);
                }

                //For OPC Group item
                _itemID = SelectedTag;
                //_clientHandle = _serverHandleList.Count;
                _clientHandle ++;

                _opcItem = _opcItems.AddItem(_itemID, _clientHandle);
                _opcItemList.Add(_opcItem);
                _serverHandleList.Add(_opcItem.ServerHandle);


                //For UI DataGrid
                if (!OpcTagItemList.Exists(i => i.ItemID == SelectedTag))
                {
                    OpcTagItemList.Add(new OpcTagItemModel()
                    {
                        ItemID = SelectedTag,
                        DataType = _opcItem.CanonicalDataType.ToString(),
                        ItemValue = 0.0f,
                        Quality = "Unknown",
                        TimeStamp = DateTime.Now.ToLocalTime().ToString("HH:mm:ss.FFF"),
                        ClientHandle = _clientHandle,
                        TransactionID = 0
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        public void RemoveOpcTagItem(int removeCount, List<OpcTagItemModel> removeList)
        {
            try
            {
                if (removeCount > 0)
                {


                    List<int> serverHandleList = new List<int>();
                    serverHandleList.Add(0);

                    foreach (var tagItem in removeList)
                    {
                        _opcItemList.FindAll(item => item.ItemID == tagItem.ItemID).ForEach(i => serverHandleList.Add(i.ServerHandle));
                        _opcItemList.FindAll(item => item.ItemID == tagItem.ItemID).ForEach(i => _serverHandleList.Remove(i.ServerHandle));
                        _opcItemList.RemoveAll(item => item.ItemID == tagItem.ItemID);

                        OpcTagItemList.RemoveAll(item => item.ItemID == tagItem.ItemID);
                    }
                    Array serverHandle = serverHandleList.ToArray();

                    _opcItems.Remove(removeCount, ref serverHandle, out Array errors);
                    _serverHandles = _serverHandleList.ToArray();

                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void ReadItemValueSync()
        {
            /*void SyncRead(short Source, int NumItems, ref Array ServerHandles, 
             * out Array Values, out Array Errors, out object Qualities, out object TimeStamps) 
             * 
             *First Parameter: OPCDataSource.OPCDevice or  OPCDataSource.OPCCache !!!
             */
            try
            {
                object qualities;
                object timeStamps;

                _serverHandles = _serverHandleList.ToArray();

                if (OpcTagItemList.Count > 0)
                {
                    _opcGroup.SyncRead((short)OPCDataSource.OPCDevice, OpcTagItemList.Count, ref _serverHandles,
                                                out Array itemValues, out Array readErrors, out qualities, out timeStamps);


                    for (int i = 1; i <= OpcTagItemList.Count; i++)
                    {
                        object itemValue = itemValues.GetValue(i);
                        if (itemValue != null)
                        {
                            OpcTagItemList[i - 1].ItemValue = Convert.ToSingle(itemValue);
                            OpcTagItemList[i - 1].TimeStamp = ((DateTime)((Array)timeStamps).GetValue(i)).ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.FFF");
                            OpcTagItemList[i - 1].Quality = ((short)((Array)qualities).GetValue(i)).ToString("X");
                            OpcTagItemList[i - 1].TransactionID = 0;
                            OpcTagItemList[i - 1].ClientHandle = i;
                        }

                    }

                    ItemValueChanged?.Invoke($"{OpcTagItemList.Count} Items ReadSync");



                }
            }

            catch (Exception)
            {

                throw;
            }

        }

        public void ReadItemValueAsync()
        {

            try
            {
                if (OpcTagItemList.Count > 0)
                {

                    _readTransactionID++;
                    _serverHandles = _serverHandleList.ToArray();


                    _opcGroup.AsyncRead(OpcTagItemList.Count, ref _serverHandles, out Array readErrors, _readTransactionID, out int readCancelID);

                    if (_readTransactionID == int.MaxValue)
                    {
                        _readTransactionID = 0;
                    }


                }

            }
            catch (Exception)
            {

                throw;
            }

        }



        private void IKepGroup_AsyncReadComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues,
                                                 ref Array Qualities, ref Array TimeStamps, ref Array Errors)
        {
            try
            {
                if (TransactionID == _readTransactionID)
                {
                    for (int i = 1; i <= NumItems; i++)
                    {
                        object itemValue = ItemValues.GetValue(i);
                        if (itemValue != null)
                        {
                            OpcTagItemList[i - 1].ItemValue = Convert.ToSingle(itemValue);
                            OpcTagItemList[i - 1].TimeStamp = ((DateTime)TimeStamps.GetValue(i)).ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.FFF");
                            OpcTagItemList[i - 1].Quality = ((int)Qualities.GetValue(i)).ToString("X");
                            OpcTagItemList[i - 1].ClientHandle = Convert.ToInt32(ClientHandles.GetValue(i));
                            OpcTagItemList[i - 1].TransactionID = TransactionID;
                        }

                    }
                    ItemValueChanged?.Invoke($"{NumItems} Items ReadAsync");
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public void WriteItemValueSync()
        {
            try
            {
                if (OpcTagItemList.Count > 0)
                {
                    _writeValueList.Clear();
                    _writeValueList.Add("0");
                    for (int i = 0; i < OpcTagItemList.Count; i++)
                    {
                        _writeValueList.Add(OpcTagItemList[i].ItemValue);
                    }
                    _writeValues = _writeValueList.ToArray();

                    _opcGroup.SyncWrite(OpcTagItemList.Count, ref _serverHandles, ref _writeValues, out Array writeErrors);

                    ItemValueChanged?.Invoke($"{OpcTagItemList.Count} Items WriteSync");
                }
            }
            catch (Exception)
            {

                throw;
            }

        }



        public void WriteItemValueAsync()
        {

            if (OpcTagItemList.Count > 0)
            {
                _writeTransactionID++;
                _writeValueList.Clear();
                _writeValueList.Add("0");

                for (int i = 0; i < OpcTagItemList.Count; i++)
                {
                    _writeValueList.Add(OpcTagItemList[i].ItemValue);
                }
                _writeValues = _writeValueList.ToArray();

                _opcGroup.AsyncWrite(OpcTagItemList.Count, ref _serverHandles, ref _writeValues, out Array writeErrors, _writeTransactionID, out int writeCancelID);

                if (_writeTransactionID == int.MaxValue)
                {
                    _writeTransactionID = 0;
                }

                ItemValueChanged?.Invoke($"{OpcTagItemList.Count} Items WriteAsync");
            }
        }
    }
}
