/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using System;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;
using System.Linq;

// This example shows how to use the Lock APIs to control coroutines in Reflective scripts.
[RegisterReflective]
public class CoroutineLockExample : SceneObjectScript
{
    // use with Reflective
    public interface IBank
    {
        void CreateAccount(string name, int balance);
        int GetBalance(string name);
        bool TransferMoney(string from, string to, int value);
        string GetReport();
    }

    // Lock requires a private object as the key.
    private object balanceLock = new object();
    private Dictionary<string, int> Balances = new Dictionary<string, int>();

    // Create a new account with a given balance
    public void CreateAccount(string name, int balance)
    {
        using (Lock(balanceLock))
        {
            Balances.Add(name, balance);
        }
    }

    // Get the balance for a specific account
    public int GetBalance(string name)
    {
        using (Lock(balanceLock))
        {
            if (Balances.ContainsKey(name))
                return Balances[name];
            else return 0;
        }
    }

    // Transfer money from one account to another
    public bool TransferMoney(string from, string to, int value)
    {
        using (Lock(balanceLock))
        {
            if (Balances.ContainsKey(from)
                && Balances.ContainsKey(to))
            {
                if (Balances[from] >= value)
                {
                    Balances[from] -= value;
                    Balances[to] += value;
                    return true;
                }
            }
            return false;
        }
    }

    // Generate a report of all balances and the totals within the lock.
    public string GetReport()
    {
        using (Lock(balanceLock))
        {
            string report = "AtomicReport: ";
            int total = 0;
            foreach (var kvp in Balances)
            {
                report += kvp.Key + "=" + kvp.Value.ToString() + " ";
                total += kvp.Value;
            }
            return report + "Total=" + total.ToString();
        }
    }
    
    public override void Init()
    {
        // no init needed. The following is an example of using the IBank:
        IBank bank = ScenePrivate.FindReflective<IBank>("CoroutineLockExample").FirstOrDefault();
        if (bank != null)
        {
            bank.CreateAccount("bob", 100);
            bank.CreateAccount("sue", 100);
            bank.CreateAccount("joe", 100);

            StartCoroutine(() =>
            {   // Send some money from joe to sue every 0.3 seconds
                while (true)
                {
                    bank.TransferMoney("joe", "sue", 3);
                    Wait(TimeSpan.FromSeconds(0.3));
                }
            });

            StartCoroutine(() =>
            {   // Send some money from sue to bob every 0.1 seconds
                while (true)
                {
                    bank.TransferMoney("sue", "bob", 1);
                    Wait(TimeSpan.FromSeconds(0.1));
                }
            });

            StartCoroutine(() =>
            {   // Send some money from sue to bob every 0.1 seconds
                while (true)
                {
                    bank.TransferMoney("bob", "joe", 2);
                    Wait(TimeSpan.FromSeconds(0.2));
                }
            });

            StartCoroutine(() =>
            {   // Send some money all around
                while (true)
                {
                    bank.TransferMoney("joe", "bob", 10);
                    bank.TransferMoney("joe", "sue", 10);
                    bank.TransferMoney("sue", "bob", 10);
                    bank.TransferMoney("sue", "joe", 10);
                    bank.TransferMoney("bob", "sue", 10);
                    bank.TransferMoney("bob", "joe", 10);
                    Wait(TimeSpan.FromSeconds(0.1));
                }
            });

            // "weekly" reports:
            StartCoroutine(() =>
            {
                while (true)
                {
                    Wait(TimeSpan.FromSeconds(300));
                    // Note that it is possible for transactions to get in between the reporting in here, due to threading.
                    // We aren't actually getting the entire report atomically here.
                    Log.Write($"Balances: Bob={bank.GetBalance("bob")} Sue={bank.GetBalance("sue")} Joe={bank.GetBalance("joe")}");
                }
            });

            // "weekly" atomic reports:
            StartCoroutine(() =>
            {
                while (true)
                {
                    Wait(TimeSpan.FromSeconds(300));
                    // This report is generated inside the lock and so should be atomic: balances should always total 300 in this example.
                    Log.Write(bank.GetReport());
                }
            });
        }
    }

}
