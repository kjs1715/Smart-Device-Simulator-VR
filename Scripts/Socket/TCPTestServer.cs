using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;


public class TCPTestServer : MonoBehaviour {  	
	#region private members 	
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener; 
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;  	
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient; 	
	#endregion

	public TMPro.TMP_Text text;
	
	public SocketEvent socketEvent;

	[SerializeField]
	public ExperimenPlatform platform;

	[SerializeField]
	public DisplayManager displayManager;

	public MathItem mathItem;

	public MathAudio mathAudio;
	public MathAudioSecond mathAudioSecond;

	public AdaptationModule adaptationModule;

	bool isLog = false;
	bool dataOutput = false;

	List<string> IPs;


	int counter = 0;
		
	// Use this for initialization
	void Start () { 		

		text.text = "";
		IPs = new List<string>();
		foreach(NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
		{
			if(ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
			{
				foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
				{
					if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
						//do what you want with the IP here... add it to a list, just get the first and break out. Whatever.
						IPs.Add(ip.Address.ToString());
						text.text += ip.Address.ToString() + '\n';
						break;
					}
				}
			}  
		}	

		// Start TcpServer background thread 		
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		tcpListenerThread.IsBackground = true; 		
		tcpListenerThread.Start(); 

		// notice system to change the display texture -> for GetComponent problem
		// if (socketEvent == null) {
		// 	socketEvent = new SocketEvent();
		// }

	

	}  	

	/*
	// Update is called once per frame
	void Update () { 		
		if (Input.GetKeyDown(KeyCode.Space)) {             
			SendMessage();         
		} 	
	}
	*/
	
	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests () { 		
		try { 			
			// Create listener on localhost port 8052. 			
			// tcpListener = new TcpListener(IPAddress.Parse(IPs[0]), 8081); 
			// tcpListener = new TcpListener(IPAddress.Parse("183.172.132.85"), 8081);
			// tcpListener = new TcpListener(IPAddress.Parse("192.168.20.126"), 8081); 	
			// tcpListener = new TcpListener(IPAddress.Parse("192.168.11.153"), 8081); 	
			tcpListener = new TcpListener(IPAddress.Parse("192.168.31.79"), 8082); 			
			// tcpListener = new TcpListener(IPAddress.Parse("192.168.31.150"), 8082); 			
		

			tcpListener.Start();              
			Debug.Log("Server is listening");              
			Byte[] bytes = new Byte[4096];  			
			while (true) { 				
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) { 					
					// Get a stream object for reading 					
					using (NetworkStream stream = connectedTcpClient.GetStream()) { 						
						int length; 						
						// Read incomming stream into byte arrary. 						
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 							
							var incommingData = new byte[length]; 							
							Array.Copy(bytes, 0, incommingData, 0, length);  							
							// Convert byte array to JSON message. 							
							String clientMessage = Encoding.UTF8.GetString(incommingData);
							// Added: convert string to JSON
							JSONObject clientMessage_json = new JSONObject(clientMessage);
							Debug.Log("client message received as: " + clientMessage_json);
							RequestBlendshapes(clientMessage_json);
						} 					
					} 				
				} 			
			} 		
		} 		
		catch (SocketException socketException) { 			
			Debug.Log("SocketException " + socketException.ToString()); 		
		}     
	}


	// Use JSON message to set facial expressions
	public void RequestBlendshapes(JSONObject blendJson)
	{
		Debug.Log("Changing blendshapes");
		//Debug.Log (blendJson);
		//Debug.Log (blendJson[3]);
		Dictionary<string, string> dictionary = blendJson.ToDictionary();
		foreach (string key in dictionary.Keys) {
			if (key.Length == 0) {
				break;
			}
			// for test
			if (key == "p") {
				mathItem.isPassed = true;
				break;
			}
			if (key == ";") {
				isLog = true;
				break;
			}
			if (key == "=") {
				dataOutput = true;
				break;
			}
			if (int.TryParse(key, out int result) && displayManager.contextManager.experimentMode) {
				displayManager.contextManager.score = result;
				displayManager.contextManager.writeData = true;
				break;
			}
			if (key == "-") {
				displayManager.contextManager.emptyDatas = true;
				break;
			}
			if (key == "e") {
				displayManager.contextManager.experimentMode = true;
				break;
			}
			if (key =="setc") {
				displayManager.contextManager.adaptationModule.SetCoefficient(dictionary[key]);
				break;
			}
			if (key == "data") {
				displayManager.contextManager.dataCount = 280;
				break;
			}
			if (key == "n") {
				mathAudio.startTask = !mathAudio.startTask;
				break;
			}
			if (key == "j") {
				mathAudioSecond.startTask = !mathAudioSecond.startTask;
				break;
			}
			if (key == "m") {
				mathAudio.checkTime = true;
				break;
			}	
			if (key == "k") {
				mathAudioSecond.checkTime = true;
				break;
			}	

			if (key == "b") {
				mathAudio.exportDic = true;
				break;
			}
			if (key == "h") {
				mathAudioSecond.exportDic = true;
				break;
			}

			if (key == "a") {
				displayManager.contextManager.userState.adapatationEnabled = !displayManager.contextManager.userState.adapatationEnabled;
				break;
			}
			if (key == "ch") {
				mathAudio.changeOrder = true;
				break;
			}
			if (key == "'") {
				mathAudio.Clear();
				mathAudioSecond.Clear();
				break;
			}
			if (key == "cond1") {
				displayManager.contextManager.userState.ChannelLimitStatus[0] = false;
				displayManager.contextManager.userState.ChannelLimitStatus[1] = false;
				displayManager.contextManager.userState.ChannelLimitStatus[2] = true;
				displayManager.contextManager.userState.ChannelLimitStatus[3] = false;
				break;
			}
			if (key == "cond2") {
				displayManager.contextManager.userState.ChannelLimitStatus[0] = true;
				displayManager.contextManager.userState.ChannelLimitStatus[1] = false;
				displayManager.contextManager.userState.ChannelLimitStatus[2] = true;
				displayManager.contextManager.userState.ChannelLimitStatus[3] = false;
				break;
			}



			// if (key == "r" || (int.TryParse(key[1].ToString(), out int result1) || int.TryParse(key[2].ToString(), out int result2))) {
			// 	break;
			// }

			platform.SocketRequestHandler(key);

		}


		

		// Tell the python client we received the message
		SendMessage();
	}


	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage() { 		
		if (connectedTcpClient == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 	
			// need delay for update names 
					
			NetworkStream stream = connectedTcpClient.GetStream(); 			
			if (stream.CanWrite) {
				// Added: create dict to be a JSON object
				Dictionary<string, JSONObject> serverMessage = new Dictionary<string, JSONObject>();
				// serverMessage["unity"] = String.Format("Unity sends its regards {0}", counter);
				if (isLog) {
					// IEnumerator t = Logger.log.GetEnumerator();
					int count = 0;
					foreach (KeyValuePair<string, string> pair in Logger.log) {
						Dictionary<string, string> deviceTexturePair = new Dictionary<string, string>();
						deviceTexturePair[pair.Key] = pair.Value;  
						JSONObject temp = new JSONObject(deviceTexturePair);
						serverMessage[count.ToString()] = new JSONObject(deviceTexturePair);
						count += 1;
					}
					isLog = false;
				} else if (dataOutput) {

					foreach (KeyValuePair<string, string> pair in displayManager.contextManager.datas) {
						Dictionary<string, string> deviceTexturePair = new Dictionary<string, string>();
						deviceTexturePair[pair.Key] = pair.Value;  
						JSONObject temp = new JSONObject(deviceTexturePair);
						serverMessage[pair.Key] = new JSONObject(deviceTexturePair);
					}

					dataOutput = false;
				} else {
					// device //TODO: now changed to data
					for (int i = 0; i < displayManager.contextManager.deviceDetector.getDeviceCount(); i++) {
						Dictionary<string, string> deviceTexturePair = new Dictionary<string, string>();
						deviceTexturePair[displayManager.contextManager.deviceDetector.devices[i].deviceName] = displayManager.contextManager.deviceDetector.devices[i].textureName;
						JSONObject temp = new JSONObject(deviceTexturePair);
						serverMessage[i.ToString()] = new JSONObject(deviceTexturePair);
					}
					int channelCount = 0;
					for (int i = displayManager.contextManager.deviceDetector.getDeviceCount(); i < displayManager.contextManager.deviceDetector.getDeviceCount() + 4; i++) {
						Dictionary<string, string> channelPair = new Dictionary<string, string>();
						channelPair[displayManager.contextManager.ChannelText[channelCount]] = displayManager.contextManager.userState.ChannelLimitStatus[channelCount].ToString();
						channelCount += 1;
						serverMessage[i.ToString()] = new JSONObject(channelPair);
					}

					//TODO: for exp2
					// MathAudio
					// int count = 0;
					// serverMessage.Clear();
					// if (mathAudio.logTimer.exportDataLog.Count != 0) {
					// 	for(int i = 0; i < mathAudio.logTimer.exportDataLog.Count; i++) {
					// 		Dictionary<string, string> channelPair = new Dictionary<string, string>();
					// 		channelPair[count.ToString()] = mathAudio.logTimer.exportDataLog[i.ToString()];
					// 		serverMessage[i.ToString()] = new JSONObject(channelPair);
					// 		count += 1;
					// 	}
					// }
					// // MathSecondAudio
					// if (mathAudioSecond.logTimer.exportDataLog.Count != 0) {
					// 	for(int i = 0; i < mathAudioSecond.logTimer.exportDataLog.Count; i++) {
					// 		Dictionary<string, string> channelPair = new Dictionary<string, string>();
					// 		channelPair[count.ToString()] = mathAudioSecond.logTimer.exportDataLog[i.ToString()];
					// 		serverMessage[i.ToString()] = new JSONObject(channelPair);
					// 		count += 1;
					// 	}
					// }
				}
				counter++;
				JSONObject serverMessage_json = new JSONObject(serverMessage);
				String serverMessage_string = serverMessage_json.ToString();
				//string serverMessage = "This is a message from your server.";  // original code
				// Debug.Log(serverMessage_string);			
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.UTF8.GetBytes(serverMessage_string);  // serverMessage				
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);               
				Debug.Log("Server sent his message - should be received by client");   
				Debug.Log(serverMessage_json);        
			}       
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		} 	
	} 

	void wait() {
		Debug.Log("Wait");
	}
}