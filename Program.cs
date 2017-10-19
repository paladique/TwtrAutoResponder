using System;
using LinqToTwitter;
using System.Threading.Tasks;
using System.Linq;

namespace AutoResponder
{
    class Program
    {
        private static TwitterContext _twitterCtx = null;

        static void Main(string[] args)
        {
            SetupTwitterClient().Wait();
        }

        private static async Task SetupTwitterClient()
        {
            SingleUserAuthorizer authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "",
                    ConsumerSecret = "",
                    AccessToken = "",
                    AccessTokenSecret = ""
                }
            };
            await authorizer.AuthorizeAsync();
            _twitterCtx = new TwitterContext(authorizer);

            //Get recipients who have already recieved auto DM
            var sentMessages = await (from dm in _twitterCtx.DirectMessage
                                      where dm.Type == DirectMessageType.SentBy
                                      && dm.Text.Contains("Replying:")
                                      select dm.RecipientID).ToListAsync();
            //Get DMs recieved 
            var recievedMessages = await (from dm in _twitterCtx.DirectMessage
                                          where dm.Type == DirectMessageType.SentTo
                                          select dm).ToListAsync();

            //Get DMs that have NOT recieved an auto DM
            var replyQueue = (from x in recievedMessages
                         where !sentMessages.Contains(x.SenderID)
                         select x);

            // Console.WriteLine("Auto messages sent: " + sentMessages.Count + " Messages Recieved: " + recievedMessages.Count + " Auto messages to queue: " + replyQueue.ToList().Count);

            foreach (var item in replyQueue)
            {
                var message = await _twitterCtx.NewDirectMessageAsync(item.SenderID, "Replying: " + DateTime.Now + "!'");    
            }

        }
    }
}
