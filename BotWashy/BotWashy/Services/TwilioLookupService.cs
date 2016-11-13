using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio.Lookups;

namespace BotWashy.Services
{
    public class TwilioLookupService
    {
        public static Number GetNumberInfo(string number)
        {
            var lookupClient = new LookupsClient(Environment.GetEnvironmentVariable("AC7287cce52cd331042c68dda863805bf6"), Environment.GetEnvironmentVariable("c90be39667768626ae38d19c75728b71"));
            var response = lookupClient.GetPhoneNumber(number, true);
            return response;
        }
    }
}