using System;
using System.IO;
using System.Xml;
using DigitalHealth.StoreAndForward.Core.Package;
using DigitalHealth.StoreAndForward.Core.Package.Models;
using Nehta.VendorLibrary.CDAPackage;

namespace DigitalHealth.StoreAndForward.Infrastructure.Package
{
    /// <summary>
    /// CDA package service.
    /// </summary>
    public class CdaPackageService : ICdaPackageService
    {
        /// <summary>
        /// Extract the package data.
        /// </summary>
        /// <param name="cdaPackageData"></param>
        /// <returns></returns>
        public CdaPackageData ExtractPackageData(byte[] cdaPackageData)
        {
            // NOTE will throw a validation exception if the signature is not valid
            CDAPackage cdaPackage = CDAPackageUtility.Extract(cdaPackageData, certificate =>
            {
                // NO-OP assume the certificate is valid
            });

            // Load the document
            XmlDocument cdaDocument = GetCdaDocumentFromPackage(cdaPackage);

            // Extract the package data
            return ExtractPackageData(cdaDocument);
        }

        private static CdaPackageData ExtractPackageData(XmlDocument cdaDocument)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(cdaDocument.NameTable);
            namespaceManager.AddNamespace("cda", "urn:hl7-org:v3");
            namespaceManager.AddNamespace("ext", "http://ns.electronichealth.net.au/Ci/Cda/Extensions/3.0");

            XmlAttribute documentIdAttribute = (XmlAttribute) cdaDocument.SelectSingleNode(
                "/cda:ClinicalDocument/cda:id/@root", 
                namespaceManager);
            if (documentIdAttribute == null)
            {
                throw new ArgumentException("No document ID found in the CDA document");
            }

            XmlAttribute ihiAttribute = (XmlAttribute) cdaDocument.SelectSingleNode(
                "/cda:ClinicalDocument/cda:recordTarget/cda:patientRole/cda:patient/ext:asEntityIdentifier[@classCode = 'IDENT']/ext:id[@assigningAuthorityName = 'IHI']/@root", 
                namespaceManager);
            if (ihiAttribute == null)
            {
                throw new ArgumentException("No IHI found in the CDA document");
            }

            string documentId = documentIdAttribute.Value;
            string ihi = ihiAttribute.Value.Replace("1.2.36.1.2001.1003.0.", "");

            return new CdaPackageData
            {
                DocumentId = documentId,
                Ihi = ihi
            };
        }

        private static XmlDocument GetCdaDocumentFromPackage(CDAPackage cdaPackage)
        {
            CDAPackageFile cdaDocumentFile = cdaPackage.CDADocumentRoot;

            XmlDocument cdaDocument = new XmlDocument();
            using (var memoryStream = new MemoryStream(cdaDocumentFile.FileContent) { Position = 0 })
            {
                cdaDocument.Load(memoryStream);
            }

            return cdaDocument;
        }
    }
}