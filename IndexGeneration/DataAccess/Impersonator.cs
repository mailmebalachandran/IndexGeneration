using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace IndexGeneration.DataAccess
{
    /// <summary>
    ///*****************************************************************************
    ///Namespace        : AccuConnect.Core
    ///Class			: Impersonator
    ///Description		: to handle events for network path
    ///$Author: $		: Sridhar Pettela
    ///List of pages this class navigates to : 
    ///$Date: $ 	    : 09/01/2009(MM/DD/YYYY)	
    ///$Modtime: $ 	    Date and time of last modification
    ///$Revision: $ 
    ///Modified by		     Date Modified		    Reason for modification
    ///
    ///
    ///*****************************************************************************
    /// </summary>

    public class Impersonator : IDisposable
    {
        #region Global Declarations

        //LoggingHelper loggingHelper = new LoggingHelper();

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// Please note that the account that instantiates the Impersonator class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        public Impersonator(
            string domainName,
            string userName,
            string password)
        {
            ImpersonateValidUser(userName, domainName, password);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// to Dispose
        /// </summary>
        public void Dispose()
        {
            UndoImpersonation();
        }
        #endregion

        #region P/Invoke.

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(
            string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(
            IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(
            IntPtr handle);

        private const int LOGON32_LOGON_INTERACTIVE = 2;    // to use with workgroup
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;    // to use with domain

        private const int LOGON32_PROVIDER_DEFAULT = 0;

        #endregion

        #region Private member.
        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        private void ImpersonateValidUser(
            string userName,
            string domain,
            string password)
        {
            WindowsIdentity tempWindowsIdentity = null;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if (RevertToSelf())
                {

                    #region InteractiveLogon for Workgroup

                    if (token == IntPtr.Zero || tokenDuplicate == IntPtr.Zero)
                    {
                        try
                        {
                            //loggingHelper.Log(LoggingLevels.Info, "Before connecting with Interactive logon.");
                            //loggingHelper.Log(LoggingLevels.Info, "Interactive logon details:" + "Domain[" + domain + "] Username[" + userName + "] Password [" + password + "]");
                            if (LogonUser(
                                userName,
                                domain,
                                password,
                                LOGON32_LOGON_INTERACTIVE,
                                LOGON32_PROVIDER_DEFAULT,
                                ref token) != 0)
                            {
                                //loggingHelper.Log(LoggingLevels.Info, "Interactive logon connected.");
                                if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                                {
                                    //loggingHelper.Log(LoggingLevels.Info, "Interactive logon Impersonate - Begin.");
                                    tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                                    impersonationContext = tempWindowsIdentity.Impersonate();
                                    //loggingHelper.Log(LoggingLevels.Info, "Interactive logon Impersonate - End.");
                                }
                                else
                                {
                                    Win32Exception win32ExceptionDup = new Win32Exception(Marshal.GetLastWin32Error());
                                    //loggingHelper.Log(LoggingLevels.Error, string.Format("Interactive Duplicate token failed, {0}.", win32ExceptionDup.Message));
                                    throw win32ExceptionDup;
                                }
                            }
                            else
                            {
                                Win32Exception win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
                                //loggingHelper.Log(LoggingLevels.Error, string.Format("Interactive logon failed, {0}.", win32Exception.Message));
                                throw win32Exception;
                            }
                        }
                        catch (Win32Exception ex1)
                        {
                            //loggingHelper.Log(LoggingLevels.Error, string.Format("Interactive logon exception, {0}.", ex1.Message));
                        }
                    }

                    #endregion

                    #region NewCredentialsLogon for Domain

                    if (token == IntPtr.Zero || tokenDuplicate == IntPtr.Zero)
                    {
                        try
                        {
                            //loggingHelper.Log(LoggingLevels.Info, "Before connecting with New Credentials logon.");
                            if (LogonUser(
                                userName,
                                domain,
                                password,
                                LOGON32_LOGON_NEW_CREDENTIALS,
                                LOGON32_PROVIDER_DEFAULT,
                                ref token) != 0)
                            {
                                ////loggingHelper.Log(LoggingLevels.Info, "New Credentials logon connected.");
                                if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                                {
                                    //loggingHelper.Log(LoggingLevels.Info, "New Credentials logon Impersonate - Begin.");
                                    tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                                    impersonationContext = tempWindowsIdentity.Impersonate();
                                    ////loggingHelper.Log(LoggingLevels.Info, "New Credentials logon Impersonate - End.");
                                }
                                else
                                {
                                    Win32Exception win32ExceptionDup = new Win32Exception(Marshal.GetLastWin32Error());
                                    //loggingHelper.Log(LoggingLevels.Error, string.Format("New Credentials Duplicate token failed, {0}.", win32ExceptionDup.Message));
                                    throw win32ExceptionDup;
                                }
                            }
                            else
                            {
                                Win32Exception win32Exception = new Win32Exception(Marshal.GetLastWin32Error());
                                //loggingHelper.Log(LoggingLevels.Error, string.Format("New Credentials logon failed, {0}.", win32Exception.Message));
                                throw win32Exception;
                            }
                        }
                        catch (Win32Exception ex1)
                        {
                            //loggingHelper.Log(LoggingLevels.Error, string.Format("New Credentials logon exception, {0}.", ex1.Message));
                            throw ex1;
                        }
                    }

                    #endregion

                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception e1)
            {
                //loggingHelper.Log(LoggingLevels.Error, "Impersonator error: " + e1.Message);
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
                if (tokenDuplicate != IntPtr.Zero)
                {
                    CloseHandle(tokenDuplicate);
                }
            }
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        private void UndoImpersonation()
        {
            if (impersonationContext != null)
            {
                impersonationContext.Undo();
            }
        }

        private WindowsImpersonationContext impersonationContext = null;

        #endregion
    }
}
