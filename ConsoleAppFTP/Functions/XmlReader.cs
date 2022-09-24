using System.Xml.Serialization;

namespace FTPForSTEAM.Functions
{
    internal class XmlReader
    {
        public static UniversalInterchange LoadUniversalInterchangeDataFromXMLString(string xmlText)
        {
            using (var stringReader = new StringReader(xmlText))
            {
                var serializer = new XmlSerializer(typeof(UniversalInterchange));
                return serializer.Deserialize(stringReader) as UniversalInterchange;
            }
        }
        public static UniversalShipmentData LoadUniversalShipmentDataFromXMLString(string xmlText)
        {
            using (var stringReader = new StringReader(xmlText))
            {
                var serializer = new XmlSerializer(typeof(UniversalShipmentData));
                return serializer.Deserialize(stringReader) as UniversalShipmentData;
            }
        }
        public static UniversalShipmentData mappingXml(UniversalShipmentData universalShipment)
        {
            UniversalShipmentData universalShipmentData = new UniversalShipmentData();

            Shipment shipment = new Shipment();
            shipment.WayBillNumber = universalShipment.Shipment.WayBillNumber;
            

            List<Shipment> subshipments = new List<Shipment>();
            foreach (Shipment item in universalShipment.Shipment.SubShipmentCollection ?? Enumerable.Empty<Shipment>())
            {
                Shipment subshipment = new Shipment();
                subshipment.WayBillNumber = item.WayBillNumber;
                subshipment.OrganizationAddressCollection = mapSubShipmentOrganizationAddress(item.OrganizationAddressCollection);
                subshipments.Add(subshipment);
            }
            shipment.SubShipmentCollection = subshipments.ToArray();

            ShipmentContainerCollection containerCollections = new ShipmentContainerCollection();
            List<Container> containers = new List<Container>();
            foreach (Container container in universalShipment.Shipment.ContainerCollection?.Container ?? Enumerable.Empty<Container>())
            {
                Container containerValue = new Container();
                containerValue.ContainerNumber = container.ContainerNumber;
                ContainerMode FCL_LCL_AIR = new ContainerMode();
                containerValue.FCL_LCL_AIR = FCL_LCL_AIR;
                containerValue.FCL_LCL_AIR.Code = container.FCL_LCL_AIR?.Code;
                containers.Add(containerValue);
            }
            containerCollections.Container = containers.ToArray();

            var transportLegCollection = universalShipment.Shipment.TransportLegCollection ?? null;
            shipment.TransportLegCollection = mapTransportLegs(transportLegCollection);
            shipment.ContainerCollection = containerCollections;
            universalShipmentData.Shipment = shipment;
            return universalShipmentData;
        }
        public static OrganizationAddress[] mapSubShipmentOrganizationAddress(OrganizationAddress[] organizationAddresses)
        {
            List<OrganizationAddress> subShipmentsOrgAddress = new List<OrganizationAddress>();
            foreach(var org in organizationAddresses)
            {
                OrganizationAddress orgAddress = new OrganizationAddress();
                if(org.AddressType == "ConsignorDocumentaryAddress")
                {
                    orgAddress.AddressType = org.AddressType;
                    subShipmentsOrgAddress.Add(orgAddress);
                }else if (org.AddressType == "ConsigneeDocumentaryAddress")
                {
                    orgAddress.AddressType = org.AddressType;
                    subShipmentsOrgAddress.Add(orgAddress);
                }
            }
            return subShipmentsOrgAddress.ToArray();
        }
        public static ShipmentTransportLegCollection mapTransportLegs(ShipmentTransportLegCollection shipmentTransportLegCollection)
        {
            ShipmentTransportLegCollection transportLegCollection = new ShipmentTransportLegCollection();
            List<TransportLeg> transportLegList = new List<TransportLeg>();
            if(shipmentTransportLegCollection == null)
            {
                return null;
            }
            foreach(TransportLeg transportLeg in shipmentTransportLegCollection?.TransportLeg ?? Enumerable.Empty<TransportLeg>())
            {
                TransportLeg transportLegValue = new TransportLeg();

                OrganizationAddress organizationAddressCarrier = new OrganizationAddress();
                transportLegValue.Carrier = organizationAddressCarrier;
                transportLegValue.Carrier.OrganizationCode = transportLeg.Carrier?.OrganizationCode;

                CodeDescriptionPair codeDescriptionPairBookingStatus = new CodeDescriptionPair();
                transportLegValue.BookingStatus = codeDescriptionPairBookingStatus;
                transportLegValue.BookingStatus.Code = transportLeg.BookingStatus?.Code;

                transportLegList.Add(transportLegValue);
            }
            transportLegCollection.TransportLeg = transportLegList.ToArray();
            return transportLegCollection;
        }
    }
}
