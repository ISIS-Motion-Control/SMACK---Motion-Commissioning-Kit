using System;
using TwinCAT;
using TwinCAT.Ads;

namespace TwinCat_Motion_ADS
{
    public class PLC : IDisposable
    {
        #region Properties
        private AdsClient _tcAds = new AdsClient();
        public AdsClient TcAds
        {
            get { return _tcAds; }
            set { _tcAds = value; }
        }

        private AdsState _adsState;
        public AdsState AdsState
        {
            get { return _adsState; }
            set { _adsState = value; }
        }

        private string _id;
        public string ID
        {
            get { return _id; }
            set 
            {
                if (checkConnection()) return;
                _id = value; 
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        #endregion

        #region Constructor
        public PLC(string amsID, int port)
        {
            ID = amsID;
            Port = port;
            try
            {
                TcAds.Connect(ID, port);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PLC constructor connect failed: {ex}");
            }
        }
        #endregion

        public bool Connect()
        {
            try
            {
                TcAds.Connect(ID, Port);
                return true;
            }
            catch
            {
                Console.WriteLine("Invalid configuration");
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                return TcAds.Disconnect();
            }
            catch
            {
                Console.WriteLine("Disconnect Failed");
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                TcAds?.Dispose();
            }
            catch
            {
                Console.WriteLine("Dispose failed");
            }
        }

        public bool checkConnection()
        {
            return TcAds.IsConnected;
        }

        public AdsState checkAdsState()
        {
            try
            {
                //Could check Run/Stop/Invalid status of this
                AdsState = TcAds.ReadState().AdsState;
                if (AdsState == AdsState.Invalid)
                    Console.WriteLine($"ADS port {Port} answered but reported state Invalid (port not created?)");
                return AdsState;
            }
            catch (AdsErrorException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReadState failed: {ex}");
                switch (ex.ErrorCode)
                {
                    case AdsErrorCode.TargetPortNotFound:    // 6
                        Console.WriteLine($"Route OK but nothing is listening on ADS port {Port} (PLC runtime not running?)"); break;
                    case AdsErrorCode.TargetMachineNotFound: // 7
                        Console.WriteLine("No route to target (check AMS Net ID and routes)"); break;
                    case AdsErrorCode.ClientSyncTimeOut:     // 1861
                        Console.WriteLine("Target didn't respond (offline, network or firewall?)"); break;
                    default:
                        Console.WriteLine($"ReadState failed: {ex.ErrorCode}"); break;
                }
                return AdsState.Invalid;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReadState failed: {ex}");
                Console.WriteLine($"ReadState failed: {ex.GetType().Name}: {ex.Message}");
                return AdsState.Invalid;
            }
        }
        public bool IsStateRun()
        {
            try
            {
                if(TcAds.ReadState().AdsState == AdsState.Run)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
