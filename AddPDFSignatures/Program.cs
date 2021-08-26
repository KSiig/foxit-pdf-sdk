using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using foxit.common;
using foxit.pdf;
using foxit;
using foxit.pdf.annots;
using foxit.common.fxcrt;
using System.Runtime.InteropServices;
using foxit.pdf.interform;

namespace AddPDFSignatures
{
    class Program
    {
        static DateTime GetLocalDateTime()
        {
            DateTimeOffset time = DateTimeOffset.Now;
            DateTime datetime = new DateTime();
            datetime.year = (UInt16)time.Year;
            datetime.month = (UInt16)time.Month;
            datetime.day = (UInt16)time.Day;
            datetime.hour = (UInt16)time.Hour;
            datetime.minute = (UInt16)time.Minute;
            datetime.second = (UInt16)time.Second;
            datetime.utc_hour_offset = (UInt16)time.Offset.Hours;
            datetime.utc_minute_offset = (UInt16)time.Offset.Minutes;
            return datetime;
        }

        static Signature AddSignature(PDFPage pdf_page, string sub_filter) {
            float page_height = pdf_page.GetHeight();
            float page_width = pdf_page.GetWidth();
            RectF new_sig_rect = new RectF(0, (float)(page_height*0.9), (float)(page_width*0.4), page_height);

            // Add a new signature to page
            Signature new_sig = pdf_page.AddSignature(new_sig_rect);
            if (new_sig.IsEmpty()) return null;

            // Set values for the new signature
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameSigner, "Foxit PDF SDK");
            String new_value = String.Format("As a sample for subfilter \"{0}\"", sub_filter);
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameReason, new_value);
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameContactInfo, "contact@example.com");
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameDN, "CN=CN,MAIL=MAIL@MAIL.COM");
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameLocation, "Fuzhou, China");
            String new_value = String.Format("As a sample for subfilter \"{0}\"", sub_filter);
            new_sig.SetKeyValue(Signature.KeyName.e_KeyNameText, new_value);
            DateTime sign_time = GetLocalDateTime();
            new_sig.SetSignTime(sign_time);
            return new_sig;
        }

        static void AdobePPKLiteSignature(PDFDoc pdf_doc) {
            string filter = "Adobe.PPKLite";
            string sub_filter = "adbe.pkcs7.detached";

            using (PDFPage pdf_page = pdf_doc.GetPage(0))
            {
                // Add a new signature to the first page
                using (Signature new_signature = AddSignature(pdf_page, sub_filter))
                {
                    new_signature.SetFilter(filter);
                    new_signature.SetSubFilter(sub_filter);

                    // Sign the new signature
                    String signed_pdf_path = "../pdf_signed.pdf";
                    String cert_file_path = "../foxit_all.pfx";
                    byte[] cert_file_password = Encoding.ASCII.GetBytes("123456");
                    new_signature.StartSign(cert_file_path, cert_file_password,
                        Signature.DigestAlgorithm.e_DigestSHA1, signed_pdf_path, IntPtr.Zero, null);
                    Console.WriteLine("[Sign] Finished!");
                }
            }
        }

        static void Main(string[] args)
        {
            string sn = "<sn>";
            string key = "<key>";
            ErrorCode error_code = Library.Initialize(sn, key);
            if (error_code != ErrorCode.e_ErrSuccess)
            {
                return;
            }

            var pdf_doc = new PDFDoc("../SignatureTest.pdf");
            pdf_doc.StartLoad(null, false, null);
            AdobePPKLiteSignature(pdf_doc);
        }
    }
}
