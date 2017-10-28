using System;
using System.Collections.Generic;
using Sovos.SvcBus.Common.Model.Capability;

namespace Sovos.SvcBus.Common.Model.Operation
{
    [SvcBusBuildable]
    public class Client
    {
        [SvcBusSerializable]
        public string Name { get; set; }

        [SvcBusSerializable]
        public string Hash { get; set; }

        [SvcBusSerializable]
        public string CID { get; set; }

        [SvcBusSerializable]
        public string License { get; set; }

        [SvcBusSerializable]
        public bool IsDemoClient { get; set; }

        [SvcBusSerializable]
        public int Taxyear { get; set; }

        [SvcBusSerializable]
        public string Domain { get; set; }

        [SvcBusSerializable]
        public string Alias { get; set; }

        [SvcBusSerializable]
        public string ProductName { get; set; }

        [SvcBusSerializable]
        public bool IsEnabled { get; set; }
    
        [SvcBusSerializable]
        public List<ClientFeature> SettingsBasedFeatures { get; set; }

        [SvcBusSerializable]
        public bool ProductHasSelectableFeatures { get; set; }

        [SvcBusSerializable]
        public string LegalEntitiesCsv { get; set; }

        public Client() { }

        public Client(string hash, string cid, string customer, string options, string license, string productName, string domain)
        {
            Hash = hash;
            CID = cid;
            Name = customer;
            IsDemoClient = (options != null && options.Contains("DEMO=Y;"));
            License = license;
            ProductName = productName;
            Domain = domain;
            SettingsBasedFeatures = new List<ClientFeature>();
            LegalEntitiesCsv = string.Empty;
        }

        public Client(string hash, string cid, string customer, string options, string license, string productName, char status, string domain)
          : this(hash, cid, customer, options, license, productName, domain)
        {
            IsEnabled = status.Equals('Y');
        }
    }
}
