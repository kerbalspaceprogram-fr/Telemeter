using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using UnityEngine;

namespace Telemeter
{
	/// <summary>
	/// My first part!
	/// </summary>
	public class Telemeter : PartModule
	{

		private const string DEFAULT_IP_ADDRESS = "127.0.0.1";
		private const int DEFAULT_PORT = 8000;
		private static string ip_address = Telemeter.DEFAULT_IP_ADDRESS;
		private static int port = Telemeter.DEFAULT_PORT;
		private bool transmission = false;
		private static string temp_ip_address = Telemeter.DEFAULT_IP_ADDRESS;
		//private static int temp_port = Telemeter.DEFAULT_PORT;
		private bool temp_transmission = false;
		private Rect windowPos;
		private static Socket socket = null;
		private Thread telemetryThread = null;

		/// <summary>
		/// Called when the part is started by Unity.
		/// </summary>
		/// <param name="state"></param>
		public override void OnStart (StartState state)
		{

			if (this.windowPos.x == 0 && this.windowPos.y == 0)
				this.windowPos = new Rect (Screen.width / 2, Screen.height / 2, 10, 10);

			if (Telemeter.socket == null)
				Telemeter.socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			this.telemetryThread = new Thread (new ThreadStart (this.PrepareTelemetry));
			this.telemetryThread.Start ();
		}

		private void PrepareTelemetry ()
		{
			try {
				while (true) {
					if (Telemeter.socket.Connected && this.transmission) {
						StringBuilder sb = new StringBuilder ();
						sb.Append (this.part.uid)
							.Append (",")
								.Append (this.vessel.name)
								.Append (",")
								.Append (this.vessel.missionTime)
								.Append (",")
								.Append (this.vessel.orbit.referenceBody.name)
								.Append (",")
								.Append (this.vessel.altitude)
								.Append (",")
								.Append (this.vessel.latitude)
								.Append (",")
								.Append (this.vessel.longitude)
								.Append (",")
								.Append (this.vessel.verticalSpeed)
								.Append (",")
								.Append (this.vessel.obt_velocity.magnitude)
								.Append (",")
								.Append (this.vessel.srf_velocity.magnitude)
								.Append (",")
								.Append (this.vessel.orbit.ApA)
								.Append (",")
								.Append (this.vessel.orbit.PeA)
								.Append (",")
								.Append (this.vessel.orbit.eccentricity)
								.Append (",")
								.Append (this.vessel.atmDensity)
								.Append (",")
								.Append (this.vessel.GetTotalMass ())
								.Append (",")
								.Append (this.part.temperature)
								.Append (",")
								.Append (this.part.dynamicPressureAtm)
								.Append (",")
								.Append (this.part.staticPressureAtm);
						SendTelemetry (sb.ToString ());
					}
					Thread.Sleep (1000);
				}
			} catch (ThreadAbortException e) {
				print (e.Message);
			}
		}

		private void SendTelemetry (string str)
		{
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes (str);
			if (Telemeter.socket.Connected)
				socket.Send (buffer, buffer.Length, SocketFlags.None);
		}

		public void DrawConfiguration ()
		{
			GUI.skin = HighLogic.Skin;
			this.windowPos = GUILayout.Window (1, this.windowPos, this.ConfigurationGUI, "Telemetry configuration", GUILayout.ExpandWidth (true), GUILayout.MinWidth (200));
		}

		public void ConfigurationGUI (int windowID)
		{

			GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("IP address :", GUILayout.ExpandWidth (true));
			Telemeter.temp_ip_address = GUILayout.TextField (Telemeter.temp_ip_address, GUILayout.ExpandWidth (true));
			GUILayout.EndHorizontal ();

			/*
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Port :", GUILayout.ExpandWidth (true));
			try {
				Telemeter.temp_port = int.Parse (GUILayout.TextField (Convert.ToString (Telemeter.temp_port), GUILayout.ExpandWidth (true)));
			} catch (Exception ex) {
				print (ex.Message);
			}
			GUILayout.EndHorizontal ();
			*/

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Status :", GUILayout.ExpandWidth (true));
			this.temp_transmission = GUILayout.Toggle (this.temp_transmission, this.temp_transmission ? "Online" : "Offline", GUILayout.ExpandWidth (true));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Cancel", GUILayout.ExpandWidth (true))) {
				RenderingManager.RemoveFromPostDrawQueue (3, new Callback (DrawConfiguration));
				Telemeter.temp_ip_address = Telemeter.ip_address;
				//Telemeter.temp_port = Telemeter.port;
				this.temp_transmission = this.transmission;
			}
			if (GUILayout.Button ("Ok", GUILayout.ExpandWidth (true))) {
				RenderingManager.RemoveFromPostDrawQueue (3, new Callback (DrawConfiguration));
				Telemeter.ip_address = Telemeter.temp_ip_address;
				Telemeter.port = Telemeter.DEFAULT_PORT;
				this.transmission = this.temp_transmission;

				IPAddress ip = IPAddress.Parse (Telemeter.ip_address);
				IPEndPoint ipEnd = new IPEndPoint (ip, Telemeter.port);
				try {
					//if (Telemeter.socket.Connected) Telemeter.socket.Disconnect(true);
					Telemeter.socket.Connect (ipEnd);
				} catch (SocketException e) {
					print ("fuck" + e.Message);
				}
			}
			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();

			GUI.DragWindow (new Rect (0, 0, 10000, 20));
		}

		[KSPEvent(guiActive = true, guiName = "Telemetry configuration")]
		public void Configuration ()
		{
			RenderingManager.AddToPostDrawQueue (3, new Callback (DrawConfiguration));
		}
	}
}

