using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Azure.WinRT.Communication;
using Azure.Communication.Calling;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ACS1on1VideoCallingAppUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //this.InitCallAgentAndDeviceManager();
        }

        private async void InitCallAgentAndDeviceManager(object sender, RoutedEventArgs e)
        {
            CallClient callClient = new CallClient();
            deviceManager = await callClient.GetDeviceManager();

            CommunicationTokenCredential token_credential = new CommunicationTokenCredential("eyJhbGciOiJSUzI1NiIsImtpZCI6IjEwNCIsIng1dCI6IlJDM0NPdTV6UENIWlVKaVBlclM0SUl4Szh3ZyIsInR5cCI6IkpXVCJ9.eyJza3lwZWlkIjoiYWNzOmU3ZDE1ODc2LTdmNWYtNDAwNS04NTBkLWM5Njg0M2M0ODY2Nl8wMDAwMDAwZC1lNzAxLTkyMTQtZjQwZi0zNDNhMGQwMDI4N2QiLCJzY3AiOjE3OTIsImNzaSI6IjE2NDg0ODQzOTIiLCJleHAiOjE2NDg1NzA3OTIsImFjc1Njb3BlIjoidm9pcCIsInJlc291cmNlSWQiOiJlN2QxNTg3Ni03ZjVmLTQwMDUtODUwZC1jOTY4NDNjNDg2NjYiLCJpYXQiOjE2NDg0ODQzOTJ9.BE2CN7BIHAaw3R1zYYv7q5jf8l3liUk2TKnsPCGKwLisT10dKxwbGd6x7sOUqUVPE-FWY3lPHO2cIoXWkeKWmHUsdPyPzsJQQcC-DTnuVBuZTAH5GqbQeYtt-aqfNg1zGapHxO7Yk_UoNbEutH7tOvomiAtNf6z-kmvT04yYpDM9SOPzAqPzLvrV-F70USNT9KHd3-oUSoRz4U-mcelSxLRHtEY_xsoeqJTtbTpob3zU43zwVAOK_iHfKtDz4xrazJlt3KL4JuAybZAEizXBG3t4-9zMc3hYtCPI3qWwWFQIEdpHZ3WGk8uPX1GSIi5O8qkj6_rEwSuefA2h2ePyCQ");

            CallAgentOptions callAgentOptions = new CallAgentOptions()
            {
                DisplayName = "Harold-test-1"
            };
            callAgent = await callClient.CreateCallAgent(token_credential, callAgentOptions);
            callAgent.OnCallsUpdated += Agent_OnCallsUpdated;
            callAgent.OnIncomingCall += Agent_OnIncomingCall;
            Debug.WriteLine("call agent initialization completed");
            
            //call automation start
            
            Debug.Assert(deviceManager.Microphones.Count > 0);
            Debug.Assert(deviceManager.Speakers.Count > 0);
            Debug.Assert(deviceManager.Cameras.Count > 0);

            if (deviceManager.Cameras.Count > 0)
            {
                VideoDeviceInfo videoDeviceInfo = deviceManager.Cameras[0];
                localVideoStream = new LocalVideoStream[1];
                localVideoStream[0] = new LocalVideoStream(videoDeviceInfo);

                //Uri localUri = await localVideoStream[0].CreateBindingAsync();

                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    LocalVideo.Source = localUri;
                //    LocalVideo.Play();
                //});

            }
            
            //Thread.Sleep(2000);

            JoinCallOptions joinCallOptions = new JoinCallOptions();
            joinCallOptions.VideoOptions = new VideoOptions(localVideoStream);
            GroupCallLocator groupmeetinglocator = new GroupCallLocator(new Guid("d031d534-a539-4c55-8f85-c00f5b9403e6"));
            call = await callAgent.JoinAsync(groupmeetinglocator, joinCallOptions);
            //call automation end
            
        }

        private async void CallButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            Debug.Assert(deviceManager.Microphones.Count > 0);
            Debug.Assert(deviceManager.Speakers.Count > 0);
            Debug.Assert(deviceManager.Cameras.Count > 0);

            if (deviceManager.Cameras.Count > 0)
            {
                VideoDeviceInfo videoDeviceInfo = deviceManager.Cameras[0];
                localVideoStream = new LocalVideoStream[1];
                localVideoStream[0] = new LocalVideoStream(videoDeviceInfo);

                Uri localUri = await localVideoStream[0].MediaUriAsync();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LocalVideo.Source = localUri;
                    LocalVideo.Play();
                });

            }

            StartCallOptions startCallOptions = new StartCallOptions();
            startCallOptions.VideoOptions = new VideoOptions(localVideoStream);
            ICommunicationIdentifier[] callees = new ICommunicationIdentifier[1]
            {
                new CommunicationUserIdentifier(CalleeTextBox.Text)
            };

            call = await callAgent.StartCallAsync(callees, startCallOptions);
        }

        private async void GroupCallButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            Debug.Assert(deviceManager.Microphones.Count > 0);
            Debug.Assert(deviceManager.Speakers.Count > 0);
            Debug.Assert(deviceManager.Cameras.Count > 0);

            if (deviceManager.Cameras.Count > 0)
            {
                VideoDeviceInfo videoDeviceInfo = deviceManager.Cameras[0];
                localVideoStream = new LocalVideoStream[1];
                localVideoStream[0] = new LocalVideoStream(videoDeviceInfo);

                //Uri localUri = await localVideoStream[0].CreateBindingAsync();
                Uri localUri = await localVideoStream[0].MediaUriAsync();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LocalVideo.Source = localUri;
                    LocalVideo.Play();
                });

            }
            
            JoinCallOptions joinCallOptions = new JoinCallOptions();
            joinCallOptions.VideoOptions = new VideoOptions(localVideoStream);
            GroupCallLocator groupmeetinglocator = new GroupCallLocator(new Guid(GroupIdTextBox.Text));
            call = await callAgent.JoinAsync(groupmeetinglocator, joinCallOptions);
            
        }

        private async void Agent_OnIncomingCall(object sender, IncomingCall incomingcall)
        {
            Debug.Assert(deviceManager.Microphones.Count > 0);
            Debug.Assert(deviceManager.Speakers.Count > 0);
            Debug.Assert(deviceManager.Cameras.Count > 0);

            if (deviceManager.Cameras.Count > 0)
            {
                VideoDeviceInfo videoDeviceInfo = deviceManager.Cameras[0];
                localVideoStream = new LocalVideoStream[1];
                localVideoStream[0] = new LocalVideoStream(videoDeviceInfo);

                Uri localUri = await localVideoStream[0].MediaUriAsync();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LocalVideo.Source = localUri;
                    LocalVideo.Play();
                });

            }
            AcceptCallOptions acceptCallOptions = new AcceptCallOptions();
            acceptCallOptions.VideoOptions = new VideoOptions(localVideoStream);

            call = await incomingcall.AcceptAsync(acceptCallOptions);
        }

        private async void Agent_OnCallsUpdated(object sender, CallsUpdatedEventArgs args)
        {
            foreach (var call in args.AddedCalls)
            {
                Debug.WriteLine("call added");
                foreach (var remoteParticipant in call.RemoteParticipants)
                {
                    Debug.WriteLine("checking participant");
                    await AddVideoStreams(remoteParticipant.VideoStreams);
                    remoteParticipant.OnVideoStreamsUpdated += async (s, a) => await AddVideoStreams(a.AddedRemoteVideoStreams);
                }
                call.OnRemoteParticipantsUpdated += Call_OnRemoteParticipantsUpdated;
                call.OnStateChanged += Call_OnStateChanged;
            }
        }

        private async void Call_OnRemoteParticipantsUpdated(object sender, ParticipantsUpdatedEventArgs args)
        {
            foreach (var remoteParticipant in args.AddedParticipants)
            {
                Debug.WriteLine("remote participant updated");
                await AddVideoStreams(remoteParticipant.VideoStreams);
                remoteParticipant.OnVideoStreamsUpdated += async (s, a) => await AddVideoStreams(a.AddedRemoteVideoStreams);
            }
        }

        private async Task AddVideoStreams(IReadOnlyList<RemoteVideoStream> streams)
        {

            foreach (var remoteVideoStream in streams)
            {
                var remoteUri = await remoteVideoStream.Start();
                Debug.WriteLine("remote participant video receiving");
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RemoteVideo.Source = remoteUri;
                    RemoteVideo.Play();
                });
                //remoteVideoStream.Start();
            }
        }

        private async void Call_OnStateChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (((Call)sender).State)
            {
                case CallState.Disconnected:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LocalVideo.Source = null;
                        RemoteVideo = null;
                    });
                    break;
                default:
                    Debug.WriteLine(((Call)sender).State);
                    break;
            }
        }

        private async void HangupButton_Click(object sender, RoutedEventArgs e)
        {
            var hangUpOptions = new HangUpOptions();
            await call.HangUpAsync(hangUpOptions);
        }

        CallClient callClient;
        CallAgent callAgent;
        Call call;
        DeviceManager deviceManager;
        LocalVideoStream[] localVideoStream;
    }
}
