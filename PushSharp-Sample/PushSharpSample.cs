using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PushSharp_Sample
{
    public class PushSharpSample
    {
        
        static ApnsConfiguration APNSConfig = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Production, "jv_push.p12", "Joy767", true);
        static string GcmSenderId = "374195981541";
        static string GcmSenderAuthToken = "AAAAVx_RjOU:APA91bF5PVyEjSC6_hv8qJgQL_MnD3jjAjAZgno_44epim9m_ygrtejYIKm63kF19sn4GdRx0QbMGpwmm5f33c5BJVfK4R4qeu7MJATXh6Bm1i2gc60mvzgMtNA0bK_1rmUxiMGRfI7o";
        private static string _gcmUrl = "https://fcm.googleapis.com/fcm/send";



        /// ================== G C M ================== 
        public static void Send_GCM(string[] recieverIds, string title, string content, string logSeparator = "\n")
        {
            // Configuration
            var config = new GcmConfiguration(GcmSenderId, GcmSenderAuthToken, null) { GcmUrl = _gcmUrl };

            // Create a new broker
            var gcmBroker = new GcmServiceBroker(config);

            // Wire up events
            gcmBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var notificationException = (GcmNotificationException)ex;

                        // Deal with the failed notification
                        var gcmNotification = notificationException.Notification;
                        var description = notificationException.Description;

                        PushLog.Write($"  GCM NOTIFICATION FAILED: ID={gcmNotification.MessageId}, Desc={description}");
                    }
                    else if (ex is GcmMulticastResultException)
                    {
                        var multicastException = (GcmMulticastResultException)ex;

                        foreach (var succeededNotification in multicastException.Succeeded)
                        {
                            PushLog.Write($"  GCM NOTIFICATION SUCCEEDED: ID={succeededNotification.MessageId}");
                        }

                        foreach (var failedKvp in multicastException.Failed)
                        {
                            var n = failedKvp.Key;
                            var e = failedKvp.Value;

                            PushLog.Write($"  GCM NOTIFICATION FAILED: ID={n.MessageId}, Desc={e}");
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        PushLog.Write($"  DEVICE REGISTRATIONID EXPIRED:\n   -   {oldId}");

                        if (!string.IsNullOrWhiteSpace(newId))
                        {
                            // If this value isn't null, our subscription changed and we should update our database
                            PushLog.Write($"  DEVICE REGISTRATIONID CHANGED TO: {newId}");
                        }
                    }
                    else if (ex is RetryAfterException)
                    {
                        var retryException = (RetryAfterException)ex;
                        // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                        PushLog.Write($"  GCM RATE LIMITED, DON'T SEND MORE UNTIL AFTER {retryException.RetryAfterUtc}");
                    }
                    else
                    {
                        PushLog.Write("  GCM NOTIFICATION FAILED FOR SOME UNKNOWN REASON");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            gcmBroker.OnNotificationSucceeded += (notification) => {
                PushLog.Write("  GCM NOTIFICATION SENT! : " + title + " :: " + content);
            };

            // Start the broker
            gcmBroker.Start();


            /// GCM doesnt care about title & content 
            PushLog.Write( "GCM SendToId: ");
            foreach (var regId in recieverIds)
            {
                if (string.IsNullOrEmpty(regId)) continue;
                NotiPayloadData payloadData = new NotiPayloadData
                {
                    aps = new Aps
                    {
                        sound = "default",
                        alert = new Alert
                        {
                            title = title,
                            body = content,
                        }
                    }
                };
                string payloadString = JsonConvert.SerializeObject(payloadData);
                var data = JObject.Parse(payloadString);
                
                // Queue a notification to send
                gcmBroker.QueueNotification(new GcmNotification
                {
                    RegistrationIds = new List<string> { regId },
                    Data = data,
                });
                PushLog.Write("\n   :   " + regId);
            }

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            gcmBroker.Stop();
        }


        /// ================== A P N S ================== 
        public static void Send_APNS(string[] recieverIds, string title, string content)
        {
            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(APNSConfig);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        PushLog.Write($"  Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}, notification.DeviceToken={notification.DeviceToken}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException			
                        PushLog.Write($"  Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                PushLog.Write("  Apple Notification Sent! : " + content);
            };

            // Start the broker
            apnsBroker.Start();


            foreach (var deviceToken in recieverIds)
            {
                if (string.IsNullOrEmpty(deviceToken)) continue;

                ///              ============ WITH ALERT
                ///             {
                ///                 "aps":{
                ///                     "alert":{
                ///                         "title":"Notification Title",
                ///                         "subtitle":"Notification Subtitle",
                ///                         "body":"This is the message body of the notification."
                ///                     },
                ///                     "badge":1
                ///                 }
                ///             }
                ///             
                ///              ============ DATA ONLY 
                ///             {
                ///                 "aps" : {
                ///                     "content-available" : 1
                ///                 },
                ///                 "TopicType" : "topictype",
                ///                 "TopicIDp" : "topicidp",
                ///                 "TopicIDp" : "topicidp",
                ///             }



                NotiPayloadData payloadData = new NotiPayloadData
                {
                    aps = new Aps
                    {
                        sound = "default",
                        alert = new Alert
                        {
                            title = title,
                            body = content,
                        }
                    }
                };
                string payloadString = JsonConvert.SerializeObject(payloadData);


                PushLog.Write("PAYLOAD STRING 1 : \n" + payloadString);
                apnsBroker.QueueNotification(new ApnsNotification
                {
                    DeviceToken = deviceToken,
                    Payload = JObject.Parse(payloadString)
                });



                /// PAYLOAD STRING 2 
                ///payloadString = "{\"aps\" : {\"content-available\" : 1}" +
                ///    ",\"TopicType\" : \"" + (int)topicType +
                ///    "\",\"TopicIDp\" : \"" + topicID.IdPrefix +
                ///    "\",\"TopicIDp\" : \"" + topicID.IdSurfix +
                ///    "\",}";
                
                ///CpT.LogTag("PAYLOAD STRING 2 : \n" + payloadString);
                ///apnsBroker.QueueNotification(new ApnsNotification
                ///{
                ///    DeviceToken = deviceToken,
                ///    Payload = JObject.Parse(payloadString)
                ///});

            }

            // Stop the broker, wait for it to finish This isn't done after every message, but after you're done with the broker
            apnsBroker.Stop();
        }

    }
}
