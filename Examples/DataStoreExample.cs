/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

// This script keeps a list of start times of every instance of the script with the supplied timeKey
public class DataStoreExample : SceneObjectScript
{
    [TooltipAttribute("Use a long phrase that is unique and hard to guess, or a generated UUID")]
    [EditorVisible(true)]
    private readonly string dataStoreId;

    [TooltipAttribute("Name to track start times under")]
    [EditorVisible(true)]
    private readonly string timeKey = "startTime";

    private DataStore dataStore;


    public override void Init()
    {
        dataStore = ScenePrivate.CreateDataStore(dataStoreId);

        if (dataStore != null)
        {
            Log.Write("DataStore id is " + dataStore.Id);
            dataStore.Restore<List<DateTime>>(timeKey, getStartTime);
        }
        else
        {
            Log.Write("Unable to create a data store with id " + dataStoreId);
        }
    }

    void startTimeAdded(DataStore.Result<List<DateTime>> result)
    {
        if (result.Success)
        {
            // If the store was successful we are done
            Log.Write($"Start time list has {result.Object.Count} entries");
        }
        else
        {
            if (result.Object != null)
            {
                // another script modified the key since it was restored
                // the new version should is here
                addStartTime(result.Object, result.Version);
            }
            else
            {
                // if the object is null, some other error occurred
                // the message will have a short description
                // the JsonString may have data, if any was returned from the database
                Log.Write($"Unexpected error: {result.Message}");
                Log.Write($"Response body:{result.JsonString}");
            }
        }

    }

    // Add the current time to the list and save to the data store
    // if the version doesn't match it means another script has updated
    // the list since it was retrieved
    void addStartTime(List<DateTime> startTimes, int version)
    {
        startTimes.Add(DateTime.Now);
        DataStore.Options options = new DataStore.Options() { Version = version };
        dataStore.Store(timeKey, startTimes, options, startTimeAdded);
    }

    void getStartTime(DataStore.Result<List<DateTime>> result)
    {
        List<DateTime> startTimes = null;
        int version = 0;
        if (result.Success)
        {
            startTimes = result.Object;
            version = result.Version;
        }
        else
        {
            //if the lookup failed, then this script is the first, so create a new list
            startTimes = new List<DateTime>();
        }

        addStartTime(startTimes, version);
    }

}
