/* THIS PROJECT IS CREATED BY LANCE SEIDMAN <lance.compulsivetech.biz> (@LanceSeidman on Twitter)
 * 
 * Description: This project is designed to allow you to communicate with an Arduino Uno (Tested)
 * using a SparkFun Blue SMiRF Silver Modem Module with a Nokia Lumia 920 Windows Phone 8 Device
 * (tested) or equivlent but is NOT compatible for Windows Phone 7.x (not-tested).
 * 
 * See it in Action!
 * http://www.youtube.com/TechMeShow - Offical Videos by Me & Demo's + Reviews
 * 
 * Donate to the Project
 * You can send a Donation to lance@compulsivetech.biz if you'd like, it's going to go towards my
 * education/retail products that I am trying to make and offer to teach people how to use Hardware
 * and Software with ANY OS that has Bluetooth.
 * 
 * GitHub Project Site
 * https://github.com/lanceseidman/Arduino-Bluetooth-WinPhone8 - ALL future Open updates for WP8
 * 
 * Arduino Supplies/Where to Buy
 * 1). Arduino Uno R3 (I got mine from: http://www.pololu.com/catalog/product/2191)
 * 2). BlueSMiRF Silver Bluetooth Modem (I got mine from: http://www.pololu.com/catalog/product/2194)
 * 3). Red LED (I got mine from: http://www.radioshack.com/product/index.jsp?productId=2103802)
 * *OPTIONAL* 
 * A). Pololu Wheel from Zumo Bot OR 22T Track Set (http://www.pololu.com/catalog/product/1415)
 * B). 50:1 Micro Metal Gearmotor HP (http://www.pololu.com/catalog/product/998)
 * 
 * Thank you/Recognition to...
 * Microsoft BizSpark Program
 * Become successful with Microsoft and its Partners... Helping Wearing Digital become a Brand.
 * 
 * Microsoft UserCommunity Team
 * Thank you for Sharing my Projects, Code, and being awesome people...
 * 
 * MSDN/Microsoft + Nokia Development Team
 * I apperciate the outline and understanding of how Bluetooth works a lot better...
 * 
 * SPHERO
 * Awesome product & thank you for your Buffer Function. More important? Making WP8 a Device that
 * can use your product. Exactly what my goal is, to get more WP8 Enabled Hardware.
 * 
 */

using Arduino2WP8.Resources;
using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Windows.Devices.Sensors;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;


namespace Arduino2WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Let's Define what we'll use throughout our App
        StreamSocket BTSock; // Socket used to Communicate with the Arduino/BT Module
        /* This is used for our Speech App
         * SpeechRecognizer mySpeech; // Used for Recognizing App Speech (not yet in this App)
         * SpeechSynthesizer mySpeechSS = new SpeechSynthesizer();
         * 
         * This is used to Grab Content from the Net, we'll be using GZIP in the end
         * WebClient wc = new WebClient(); // Setting up our WebClient so we can just use "wc" and could be used to get an API
        */

        // Let's Store our Strings
        string BTStatus = ""; // Used to Store if we can send Message (e.g. yes or no)
        /*string BT_Received = ""; // We'll use to store Bluetooth Received Data
        string whattosay = ""; // Used later to accept input for Speech */

        // Constructor
        public MainPage()
        {
            InitializeComponent(); // NEVER REMOVE!

            // Let's make sure the Emulator isn't loaded...
            if(Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator)
            {
                // Send an Error Message to User
                MessageBox.Show("Sorry, Bluetooth isn't compatible in this enviornment. Please use your Phone.", "Error: Device Required", MessageBoxButton.OK);
                return; // Close
            }

            Loaded +=MainPage_Loaded; // We need Async, so Use _Loaded
           // PeerFinder.TriggeredConnectionStateChanged += PeerFinder_TriggeredConnectionStateChanged; // Check Connection State

        }

        private async void  MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PeerFinder.Start();
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = ""; // Find/Get All Paired BT Devices
            var peers = await PeerFinder.FindAllPeersAsync(); // Make peers the container for All BT Devices
            txtBTStatus.Text = "Finding Paired Devices..."; // Tell UI what is going on in case it's Slow

            // Only want 1 Device to Show? Uncomment Below
            // lstBTPaired.Items.Add(peers[0].DisplayName); // 1 Paired Device to Show 

            // Show only Specific Device
            // peers[0].DisplayName.Contains("RN42-5");

            // Let's show only the first 2 Devices Paired
            for (int i = 0; i < peers.Count; i++)
            {
                lstBTPaired.Items.Add(peers[i].DisplayName);
            }
            if (peers.Count <= 2)
            {
                txtBTStatus.Text = "Found " + peers.Count + " Devices";
            }

        }

        private async void BT2Arduino_Send (string WhatToSend) 
        {
            if (BTSock == null) // If we don't have a connection, Send Error Control
            {
               // MessageBox.Show("Please connect to a device first."); // Alert the user with a Notification (Optional)
                txtBTStatus.Text = "No connection found. Try again!"; // Alert the UI
                return; // Stop
            }
            else
                if (BTSock != null) // Since we have a Connection
                {
                    var datab = GetBufferFromByteArray(UTF8Encoding.UTF8.GetBytes(WhatToSend)); // Create Buffer/Packet for Sending
                    await BTSock.OutputStream.WriteAsync(datab); // Send our Message to Connected Arduino
                    txtBTStatus.Text = "Message Sent (" + WhatToSend + ")"; // Show what we sent to Device to UI
                   
                }
        }

        // FUNCTION PROVIDED BY SPHERO
        private IBuffer GetBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }

        /* Used for our Speech App
         * private async void GoSpeak()
        {
            // Assuming we set what we wanted to Say, this will Speak it (e.g. whattosay = "Hello everyone!";)
            await mySpeechSS.SpeakTextAsync(whattosay);
            
        }*/

        private async void lstBTPaired_Tap_1(object sender, GestureEventArgs e)
        {
            if (lstBTPaired.SelectedItem == null) // To prevent errors, make sure something is Selected
            {
                //btnConnectArduino.IsEnabled = false; // Make sure it's False if you want to use a Button
                txtBTStatus.Text = "No Device Selected! Try again..."; // Set UI Output
                return;
            }
            else
                if (lstBTPaired.SelectedItem != null) // Just making sure something was Selected
                {

                    // btnConnectArduino.IsEnabled = true; // Since an item is Selected, Enable Connect Button (If using a Button)


                    /* This is a trick to Grab the Item and Remove '(' and ')' if using the Hostname & want just the Contents (00:00:00)
                    // Of course we don't HAVE to do this, but this is a C# Trick/Hack to learn String Functions
                    string ba = lstBTPaired.SelectedItem.ToString(); // Store the Tapped/Selected Item
                    int found = 0; // Set the Found to 0
                    found = ba.IndexOf("("); // Let's get the Index of the "(" in the String (ba)
                    ba = ba.Substring(found + 1); // Use Substring with the IndexOf
                    ba = ba.Replace(")", ""); // Now remove the last ")" in the String to be "00:00:00:00:00"
                    // Test our Hack by Uncommenting Below...
                    //MessageBox.Show(ba); - This is just to make sure we did it right */

                    PeerFinder.AlternateIdentities["Bluetooth:Paired"] = ""; // Grab Paired Devices
                    var PF = await PeerFinder.FindAllPeersAsync(); // Store Paired Devices

                    BTSock = new StreamSocket(); // Create a new Socket Connection
                    await BTSock.ConnectAsync(PF[lstBTPaired.SelectedIndex].HostName, "1"); // Connect using Socket to Selected Item

                    // Once Connected, let's give Arduino a HELLO
                    var datab = GetBufferFromByteArray(Encoding.UTF8.GetBytes("HELLO")); // Create Buffer/Packet for Sending
                    await BTSock.OutputStream.WriteAsync(datab); // Send Arduino Buffer/Packet Message

                    btnSendCommand.IsEnabled = true; // Allow commands to be sent via Command Button (Enabled)
                }
        }


        void PeerFinder_TriggeredConnectionStateChanged(object sender, TriggeredConnectionStateChangedEventArgs args)
        {
            // This will be used to Get Data from our Hardware soon

            if (args.State == TriggeredConnectState.Failed)
            {
                txtBTStatus.Text = "Failed to Connect... Try again!";
                BTStatus = "no"; // Not connected
                return;
            }

            if (args.State == TriggeredConnectState.Completed)
            {
                txtBTStatus.Text = "Connected!";
                BTStatus = "yes"; // Means we are connected
                MessageBox.Show("hi");
            }
        }

        private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            // In this Demo, our Arduino code knows to look for "3" to turn off/on LED/Motor for 4 Seconds
            BT2Arduino_Send("3"); // This will send using the GoSend Feature
        }

    }
}
