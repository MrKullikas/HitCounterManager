﻿//MIT License

//Copyright(c) 2016-2018 Peter Kirmeier

//Permission Is hereby granted, free Of charge, to any person obtaining a copy
//of this software And associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
//copies of the Software, And to permit persons to whom the Software Is
//furnished to do so, subject to the following conditions:

//The above copyright notice And this permission notice shall be included In all
//copies Or substantial portions of the Software.

//THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
//IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
//LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
//OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;

namespace HitCounterManager
{
    /// <summary>
    /// A row as part of a profile (used for a row at datagridview)
    /// </summary>
    [Serializable]
    public class ProfileRow
    {
        public string Title = "";
        public int Hits = 0;
        public int Diff = 0;
        public int PB = 0;
    }

    /// <summary>
    /// Single profile (used for a whole datagridview data collection)
    /// </summary>
    [Serializable]
    public class Profile
    {
        public string Name;
        public int Attempts;
        public List<ProfileRow> Rows = new List<ProfileRow>();
    }

    /// <summary>
    /// Manages multiple profiles
    /// </summary>
    [Serializable]
    public class Profiles
    {
        private List<Profile> _Profiles = new List<Profile>();
        public List<Profile> ProfileList { get { return _Profiles; } } // used by XML serialization!

        /// <summary>
        /// Checks if a profile with given name exists and gets its instance
        /// </summary>
        private bool _FindProfile(string Name, out Profile profile)
        {
            foreach (Profile prof in _Profiles)
            {
                if (prof.Name == Name)
                {
                    profile = prof;
                    return true;
                }
            }
            profile = null;
            return false;
        }
        
        /// <summary>
        /// Returns a list of all available internally cached profile names
        /// </summary>
        public object[] GetProfileList()
        {
            List<string> ret = new List<string>();
            foreach (Profile prof in _Profiles) ret.Add(prof.Name);
            return ret.ToArray();
        }

        /// <summary>
        /// Updates profile info based on a specific internally cached profile
        /// </summary>
        /// <param name="Name">Name of profile that gets loaded</param>
        /// <param name="pi">Interface to Profile Info</param>
        public void LoadProfile(string Name, IProfileInfo pi)
        {
            Profile prof;
            pi.SetProfileName(Name);
            pi.SetAttemptsCount(0);
            pi.ClearSplits();
            if (_FindProfile(Name, out prof))
            {
                pi.SetAttemptsCount(prof.Attempts);
                foreach (ProfileRow row in prof.Rows)
                {
                    pi.AddSplit(row.Title, row.Hits, row.PB);
                }
            }
            pi.SetSessionProgress(0);
        }

        /// <summary>
        /// Updates internally cached profile with data from profile info
        /// </summary>
        /// <param name="pi">Interface to Profile Info</param>
        /// <param name="AllowCreation">True allows to add a new profile when it does not exist in cache already</param>
        public void SaveProfile(IProfileInfo pi, bool AllowCreation = false)
        {
            Profile prof;

            // look for existing one and create if not exists
            if (!_FindProfile(pi.GetProfileName(), out prof))
            {
                if (!AllowCreation) return;

                prof = new Profile();
                prof.Name = pi.GetProfileName();
                _Profiles.Add(prof);
            }

            prof.Rows.Clear();

            // collecting data, nom nom nom
            prof.Attempts = pi.GetAttemptsCount();
            for (int r = 0; r < pi.GetSplitCount(); r++)
            {
                ProfileRow ProfileRow = new ProfileRow();
                ProfileRow.Title = pi.GetSplitTitle(r);
                ProfileRow.Hits = pi.GetSplitHits(r);
                ProfileRow.Diff = pi.GetSplitDiff(r);
                ProfileRow.PB = pi.GetSplitPB(r);
                prof.Rows.Add(ProfileRow);
            }
        }

        /// <summary>
        /// Removes a specific profile from internal cache
        /// </summary>
        public void DeleteProfile(string Name)
        {
            Profile prof;
            if (_FindProfile(Name, out prof)) _Profiles.Remove(prof); 
        }

        /// <summary>
        /// Renames a specific profile in internal cache
        /// </summary>
        public void RenameProfile(string NameOld, string NameNew)
        {
            Profile prof;
            if (_FindProfile(NameOld, out prof)) prof.Name = NameNew;
        }
    }

    /// <summary>
    /// Interface to maintain the currently loaded profile data
    /// </summary>
    public interface IProfileInfo
    {
        /// <summary>
        /// Gets the name of the currently loaded profile
        /// </summary>
        /// <returns>Profile name</returns>
        string GetProfileName();
        /// <summary>
        /// Sets the name of the currently loaded profile
        /// </summary>
        /// <param name="Name">Profile name</param>
        void SetProfileName(string Name);

        /// <summary>
        /// Gets amount of splits
        /// </summary>
        /// <returns>Split count</returns>
        int GetSplitCount();

        /// <summary>
        /// Gets the split index of the currently active one
        /// </summary>
        /// <returns>Index of current split</returns>
        int GetActiveSplit();
        /// <summary>
        /// Sets the split as active
        /// </summary>
        void SetActiveSplit(int Index);

        /// <summary>
        /// Removes all splits
        /// </summary>
        void ClearSplits();
        /// <summary>
        /// Add split
        /// </summary>
        /// <param name="Title">Title</param>
        /// <param name="Hits">Amount of hits</param>
        /// <param name="PB">Amount of personal best hits</param>
        void AddSplit(string Title, int Hits, int PB);

        /// <summary>
        /// Gets the amount of attempts
        /// </summary>
        /// <returns>Count of tries</returns>
        int GetAttemptsCount();
        /// <summary>
        /// Sets the amount of attempts
        /// </summary>
        /// <param name="Attempts">Count of tries</param>
        void SetAttemptsCount(int Attempts);

        /// <summary>
        /// Gets the split index of the session progress
        /// </summary>
        /// <returns>Session progress</returns>
        int GetSessionProgress();
        /// <summary>
        /// Sets the split index of the session progress
        /// Can only be higher unless AllowReset is set
        /// </summary>
        /// <param name="Index">Index of the new session progress</param>
        /// <param name="AllowReset">True allows to set lower values</param>
        void SetSessionProgress(int Index, bool AllowReset = false);

        /// <summary>
        /// Gets the title of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <returns>Title</returns>
        string GetSplitTitle(int Index);
        /// <summary>
        /// Gets the hit counts of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <returns>Amount of hits</returns>
        int GetSplitHits(int Index);
        /// <summary>
        /// Gets the hit difference of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <returns>Hit count difference</returns>
        int GetSplitDiff(int Index);
        /// <summary>
        /// Gets the person best hit counts of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <returns>Amount of personal best hits</returns>
        int GetSplitPB(int Index);

        /// <summary>
        /// Sets the title of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <param name="Title">Title</param>
        void SetSplitTitle(int Index, string Title);
        /// <summary>
        /// Sets the hit counts of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <param name="Hits">Amount of hits</param>
        void SetSplitHits(int Index, int Hits);
        /// <summary>
        /// Sets the hit difference of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <param name="Diff">Hit count difference</param>
        void SetSplitDiff(int Index, int Diff);
        /// <summary>
        /// Sets the person best hit counts of a split
        /// </summary>
        /// <param name="Index">Index</param>
        /// <param name="PBHits">Amount of personal best hits</param>
        void SetSplitPB(int Index, int PBHits);

        /// <summary>
        /// Checks if any value of the Profile Info has been changed
        /// </summary>
        /// <param name="Reset">True will reset to "unchanged"</param>
        /// <returns>Profile Info was modified since last reset</returns>
        bool HasChanged(bool Reset = false);
    }
}
