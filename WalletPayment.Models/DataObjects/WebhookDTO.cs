using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Authorization
    {
        public string authorization_code { get; set; }
        public string bin { get; set; }
        public string last4 { get; set; }
        public string exp_month { get; set; }
        public string exp_year { get; set; }
        public string channel { get; set; }
        public string card_type { get; set; }
        public string bank { get; set; }
        public string country_code { get; set; }
        public string brand { get; set; }
        public bool reusable { get; set; }
        public string signature { get; set; }
        public string account_name { get; set; }
    }

    public class Customer
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string customer_code { get; set; }
        public string phone { get; set; }
        public object? metadata { get; set; }
        public string risk_action { get; set; }
        public string international_format_phone { get; set; }
    }

    public class EventData
    {
        public long id { get; set; }
        public string domain { get; set; }
        public string status { get; set; }
        public string reference { get; set; }
        public int amount { get; set; }
        public object? message { get; set; }
        public string gateway_response { get; set; }
        public DateTime paid_at { get; set; }
        public DateTime created_at { get; set; }
        public string channel { get; set; }
        public string currency { get; set; }
        public string ip_address { get; set; }
        public string metadata { get; set; }
        public object? fees_breakdown { get; set; }
        public object? log { get; set; }
        public int fees { get; set; }
        public object? fees_split { get; set; }
        public Authorization authorization { get; set; }
        public Customer customer { get; set; }
        public Plan plan { get; set; }
        public Subaccount subaccount { get; set; }
        public Split split { get; set; }
        public object? order_id { get; set; }
        public DateTime paidAt { get; set; }
        public int requested_amount { get; set; }
        public object? pos_transaction_data { get; set; }
        public Source source { get; set; }
    }

    public class Plan
    {
    }

    public class WebhookDTO
    {
        public string @event { get; set; }
        public EventData data { get; set; }
    }

    public class Source
    {
        public string type { get; set; }
        public string source { get; set; }
        public string entry_point { get; set; }
        public object? identifier { get; set; }
    }

    public class Split
    {
    }

    public class Subaccount
    {
    }


}
