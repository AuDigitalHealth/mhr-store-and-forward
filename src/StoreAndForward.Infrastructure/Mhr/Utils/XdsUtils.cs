using System.Linq;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr.Models;
using Nehta.VendorLibrary.PCEHR.DocumentRepository;

namespace DigitalHealth.StoreAndForward.Infrastructure.Mhr.Utils
{
    /// <summary>
    /// XSD utils.
    /// </summary>
    public static class XdsUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="submitObjectsRequest"></param>
        /// <returns></returns>
        public static PcehrHeaderData ExtractPcehrHeaderData(SubmitObjectsRequest submitObjectsRequest)
        {
            ExtrinsicObjectType extrinsicObject = submitObjectsRequest.RegistryObjectList.ExtrinsicObject.Single();

            ClassificationType classification = extrinsicObject.Classification.Single(
                a => a.classificationScheme == "urn:uuid:93606bcf-9494-43ec-9b4e-a7748d1a838d");

            // Organisation
            SlotType1 authorInstitutionSlot = classification.Slot.Single(s => s.name == "authorInstitution");
            string authorInstitutionValue = authorInstitutionSlot.ValueList.Value.Single();
            string[] authorInstitutionValues = authorInstitutionValue.Split('^');
            string hpioOid = authorInstitutionValues.Last();
            string hpio = hpioOid.Split('.').Last();
            string organisationName = authorInstitutionValues.First();

            // Provider
            SlotType1 authorPersonSlot = classification.Slot.Single(s => s.name == "authorPerson");

            string[] authorPersonValues = authorPersonSlot.ValueList.Value.First().Split('^');

            // Last name (mandatory)
            string fullName = authorPersonValues[1];

            // Check if first name has been specified
            if (!string.IsNullOrEmpty(authorPersonValues[2]))
            {
                fullName = $"{authorPersonValues[2]} {fullName}";
            }

            // Provider ID
            string[] idValues = authorPersonValues.Last().Split('&');
            string id;
            bool isLocalId;
            if (string.IsNullOrEmpty(idValues[0]))
            {
                isLocalId = false;
                id = idValues[1].Split('.').Last();
            }
            else
            {
                isLocalId = true;
                id = idValues[1];
            }
           
            // IHI
            ExternalIdentifierType externalIdentifier = extrinsicObject.ExternalIdentifier.Single(
                e => e.identificationScheme == "urn:uuid:58a6f841-87b3-4a3e-92fd-a8ffeff98427");
            string ihi = externalIdentifier.value.Split('^').First();

            return new PcehrHeaderData
            {
                Ihi = ihi,
                ProviderName = fullName,
                IsProviderIdLocal = isLocalId,
                ProviderId = id,
                Hpio = hpio,
                OrganisationName = organisationName
            };
        }
    }
}
