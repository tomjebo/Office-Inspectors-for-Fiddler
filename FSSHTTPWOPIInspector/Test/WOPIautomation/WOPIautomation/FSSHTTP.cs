﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.IE;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.CSharp;
using System.Runtime.InteropServices;
using System.Threading;
using OpenQA.Selenium.Support.Events;
using System.Windows.Forms;

namespace WOPIautomation
{
    [TestClass]
    public class FSSHTTP:TestBase
    {
        private static string Word = ConfigurationManager.AppSettings["Word"];
        private static string filename = Word.Split('\\').Last().Split('.').First();
		private string file = "";
        
     
        [TestMethod, TestCategory("FSSHTTP")]
        public void CoautherWithoutConflict()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Open document by office word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            // Wait for document is opened
            Utility.WaitForDocumentOpenning(filename);
            // Get the opened word process, and edit it
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertAfter("HelloWord");

            // Double click the document in root site 
            Browser.Click(document);
            // Find and click "Edit Document" tab
            var editWord = Browser.FindElement(By.XPath("//a[@id='flyoutWordViewerEdit-Medium20']"), false);
            editWord.SendKeys(OpenQA.Selenium.Keys.Enter);
            SendKeys.SendWait("Enter");
            // Find and click "Edit in Browser" tab
            var editInbrowser = Browser.webDriver.FindElement(By.XPath("//a[@id ='btnFlyoutEditOnWeb-Menu32']"));
            editInbrowser.SendKeys(OpenQA.Selenium.Keys.Enter);
            SendKeys.SendWait("Enter");
            // Wait for document is opened
            Browser.Wait(By.XPath("//span[@id='BreadcrumbSaveStatus'][text()='Saved']"));
            oDocument.Save();
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            // Refresh web address
            Browser.Goto(Browser.BaseAddress);
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void CoautherWithConflict()
        {
            //Turn to ClassicMode
            Thread.Sleep(2000);
            Browser.Wait(By.CssSelector("span.LeftNav-linkText"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.CssSelector("span.LeftNav-linkText"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Open document by office word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            // Wait for document is opened
            Utility.WaitForDocumentOpenning(filename);
            // Get the opened word process, and edit it
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("HelloWordConfilict");
            // Double click the document in root site 
            Browser.Click(document);
            // Find and click "Edit Document" tab
            var editWord = Browser.FindElement(By.XPath("//a[@id='flyoutWordViewerEdit-Medium20']"), false);
            editWord.SendKeys(OpenQA.Selenium.Keys.Enter);
            SendKeys.SendWait("Enter");
            // Find and click "Edit in Browser" tab
            var editInbrowser = Browser.webDriver.FindElement(By.XPath("//a[@id ='btnFlyoutEditOnWeb-Menu32']"));
            editInbrowser.SendKeys(OpenQA.Selenium.Keys.Enter);
            SendKeys.SendWait("Enter");
            // Wait for document is opened
            Thread.Sleep(2000);
            Browser.Wait(By.XPath("//span[@id='BreadcrumbSaveStatus'][text()='Saved']"));
            // Edit it in online
            SendKeys.SendWait("HelloOfficeOnlineConflict");
            // Wait for online edit saved
            Thread.Sleep(2000);
            Browser.Wait(By.XPath("//span[@id='BreadcrumbSaveStatus'][text()='Saved']"));
            //saved = Browser.FindElement(By.XPath("//span[@id='BreadcrumbSaveStatus']"), false);
            Thread.Sleep(60000);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Save it in office word and close and release word process
            Utility.WordEditSave(filename);
            Thread.Sleep(10000);
            Utility.CloseMicrosoftWordDialog(filename,"OK");
            //Utility.WordConflictMerge(filename);
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");

        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void Schemalock()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            Browser.RClick(document);
            // Open document in Edit Word mode
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            Utility.WaitForDocumentOpenning(filename);
            // Update the document content
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("Schemalock");
            // Save and close document
            oDocument.Save();
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void Exclusivelock()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Checkout the document
            SharepointClient.LockItem(filename + ".docx");
            // Open it in office word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            // Wait for document is opened
            Utility.WaitForDocumentOpenning(filename);
            // Update the document content
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("Exclusivelock");
            // Save and close and release word process
            oDocument.Save();
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            SharepointClient.UnLockItem(filename + ".docx");
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void SchemalockToExclusivelock()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Open it in office word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            Utility.CloseMicrosoftOfficeDialog();
            // Sign in office word and wait for it opening
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            Utility.WaitForDocumentOpenning(filename);
            // Update the document content
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("SchemalockToExclusivelock");
            // Save and close and release word process
            oDocument.Save();
            Utility.CheckOutOnOpeningWord(filename);
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            SharepointClient.UnLockItem(filename + ".docx");
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void ExclusiveLockGetlock()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Open it in office word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["UserName"];
            string password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            Utility.WaitForDocumentOpenning(filename);
            // Check Out it from the info pag
            Utility.CheckOutOnOpeningWord(filename);
            // Update the document content
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("Exclusivelock");
            oDocument.Save();
            // Close the document
            Utility.CloseDocumentByUI(filename);
            Utility.CloseMicrosoftWordDialog(filename,"Yes");
            Utility.CloseCheckInPane(filename,true);
            // Go back to base address
            Browser.Goto(Browser.DocumentAddress);
            // Reopen document in office word
            document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementToOpen = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementToOpen);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            username = ConfigurationManager.AppSettings["UserName"];
            password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            Utility.WaitForDocumentOpenning(filename);
            // Edit it 
            wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Content.InsertBefore("ExclusiveLockGetlock");
            // Save and close word process
            oDocument.Save();
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            SharepointClient.UnLockItem(filename + ".docx");
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void SchemalockCheck()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            // Checked out it
            SharepointClient.LockItem(filename + ".docx");
            // Open it by word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            Utility.CloseMicrosoftOfficeDialog();
            // Sign in office word with another account and wait for it opening in readonly mode
            string username = ConfigurationManager.AppSettings["OtherUserName"];
            string password = ConfigurationManager.AppSettings["OtherPassword"];
            Utility.OfficeSignIn(username, password);
            Utility.CloseFileInUsePane(filename);
            Utility.WaitForDocumentOpenning(filename, true);
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            // Wait for CheckLockAvailability
            Thread.Sleep(60000);
            // Close and release word process
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            SharepointClient.UnLockItem(filename + ".docx");
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }

        [TestMethod, TestCategory("FSSHTTP")]
        public void ExclusivelockCheck()
        {
            //Turn to ClassicMode
            Thread.Sleep(5000);
            Browser.Wait(By.XPath("//div[2]/div/div/div/a/span"));
            var turntoClassicMode = Browser.webDriver.FindElement(By.XPath("//div[2]/div/div/div/a/span"));
            Browser.Click(turntoClassicMode);
            // Upload a document
            SharepointClient.UploadFile(Word);
            // Refresh web address
            Browser.Goto(Browser.DocumentAddress);
            // Find document on site
            IWebElement document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" +filename+ ".docx']"));
            // Open it by word
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            var elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            string username = ConfigurationManager.AppSettings["OtherUserName"];
            string password = ConfigurationManager.AppSettings["OtherPassword"];
            Utility.OfficeSignIn(username, password);
            Utility.WaitForDocumentOpenning(filename);
            // Check it out in info page
            Utility.CheckOutOnOpeningWord(filename);
            // Close word process
            Word.Application wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            Word.Document oDocument = (Word.Document)wordToOpen.ActiveDocument;
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            // Go back to base address
            Browser.Goto(Browser.DocumentAddress);
            // Reopen the document in word
            document = Browser.webDriver.FindElement(By.CssSelector("a[href*='" + filename + ".docx']"));
            Browser.RClick(document);
            Browser.Wait(By.LinkText("Open in Word"));
            elementOpenInWord = Browser.webDriver.FindElement(By.LinkText("Open in Word"));
            Browser.Click(elementOpenInWord);
            // Close Microsoft office dialog and access using expected account
            Utility.CloseMicrosoftOfficeDialog();
            username = ConfigurationManager.AppSettings["UserName"];
            password = ConfigurationManager.AppSettings["Password"];
            Utility.OfficeSignIn(username, password);
            Utility.CloseFileInUsePane(filename);
            Utility.WaitForDocumentOpenning(filename, true);
            
            wordToOpen = (Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
            oDocument = (Word.Document)wordToOpen.ActiveDocument;
            // CheckLockAvailability
            Thread.Sleep(60000);
            // Close and release word process
            Utility.CloseFileNowAvailable(filename);
            oDocument.Close();
            Utility.DeleteDefaultWordFormat();
            Marshal.ReleaseComObject(oDocument);
            Marshal.ReleaseComObject(wordToOpen);
            SharepointClient.UnLockItem(filename + ".docx");
            // Delete the new upload document
            SharepointClient.DeleteFile(filename + ".docx");

            bool result = FormatConvert.SaveSAZ(TestBase.testResultPath, testName, out file);
            Assert.IsTrue(result, "The saz file should be saved successfully.");
            bool parsingResult = MessageParser.ParseMessageUsingWOPIInspector(file);
            Assert.IsTrue(parsingResult, "Case failed, check the details information in error.txt file.");
        }
    }
}
