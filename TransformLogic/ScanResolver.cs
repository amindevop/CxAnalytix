﻿using log4net;
using System;
using System.Collections.Generic;
using CxAnalytix.TransformLogic.Data;
using CxRestClient;

namespace CxAnalytix.TransformLogic
{

    /// <summary>
    /// The class that determines which scans need to be transformed during this
    /// run of the data resolution.
    /// </summary>
    public class ScanResolver
    {
        private static ILog _log = LogManager.GetLogger(typeof(ScanResolver));

        private bool _disallowAdds = false;

        private ProjectResolver _state;
        internal ScanResolver(ProjectResolver projectState, 
            Dictionary<String, Action<ScanDescriptor, Transformer>> productAction)
        {
            _state = projectState;
            _productAction = productAction;
        }

        private Dictionary<String, Action<ScanDescriptor, Transformer>> _productAction;
        private Dictionary<String, ScanDescriptor> _scans = new Dictionary<string, ScanDescriptor>();
        private Dictionary<int, int> _projectCount = new Dictionary<int, int>();

        private void IncrementProjectCount(int projectId)
        {
            if (!_projectCount.ContainsKey(projectId))
                _projectCount.Add(projectId, 0);

            _projectCount[projectId] = _projectCount[projectId] + 1;
        }


        /// <summary>
        /// Adds a scan to the list of scans for checking if the scan needs to be
        /// transformed.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="scanType"></param>
        /// <param name="scanProduct"></param>
        /// <param name="scanId"></param>
        /// <param name="finishTime"></param>
        /// <returns>Returns true if the project can be added, false otherwise.</returns>
        public bool AddScan(int projectId, String scanType, String scanProduct, String scanId, DateTime finishTime)
        {
            if (scanType == null || scanProduct == null || scanId == null)
                return false;

            if (!_disallowAdds)
            {
                if (!_state.Targets.ContainsKey(projectId))
                {
                    _log.WarnFormat("Attempted to add a scan for unknown project {0}.", projectId);
                    return false;
                }

                if (_scans.ContainsKey(scanId))
                {
                    _log.WarnFormat("Attempted to add a duplicate scan id [{0}] for project {1}:[{2}].", scanId, projectId,
                        _state.Targets[projectId].ProjectName);

                    return false;
                }

                if (_scans.ContainsKey(scanId))
                    return false;

                // Always update the last scan time for a product if there was a scan for it.
                _state.Targets[projectId].UpdateLatestScanDate(scanProduct, finishTime);

                if (_state.Targets[projectId].LastScanCheckDate.CompareTo(finishTime) < 0)
                {
                    _state.Targets[projectId].IncrementScanCount(scanProduct);
                    IncrementProjectCount(projectId);

                    _scans.Add(scanId, new ScanDescriptor()
                    {
                        Project = _state.Targets[projectId],
                        ScanType = scanType,
                        ScanProduct = scanProduct,
                        ScanId = scanId,
                        FinishedStamp = finishTime,
                        MapAction = _productAction[scanProduct]
                    });
                }
            }

            return !_disallowAdds;
        }

        /// <summary>
        /// The count of scans after <see cref="ScanResolver.Resolve"/> has been called.  Returns
        /// null until <see cref="ScanResolver.Resolve"/> is called.
        /// </summary>
        public int? ResolvedScanCount
        {
            get
            {
                if (!_disallowAdds)
                    return null;

                return _scans.Keys.Count;
            }
        }

        /// <summary>
        /// The count of projects after <see cref="ScanResolver.Resolve"/> has been called.  Returns
        /// null until <see cref="ScanResolver.Resolve"/> is called.
        /// </summary>
        public int? ResolvedProjectCount
        {
            get
            {
                if (!_disallowAdds)
                    return null;

                return _projectCount.Keys.Count;
            }
        }

        /// <summary>
        /// Resolves all the scans that need to be checked based on the
        /// state of the previous check and the scans the were added
        /// using the method <see cref="AddScan"/>.
        /// </summary>
        /// <param name="lastCheckDate">The date to store with each project that is the last time the
        /// project was checked for new scans.</param>
        /// <returns>An enumeration of scans that should be exported.</returns>
        public IEnumerable<ScanDescriptor> Resolve (DateTime lastCheckDate)
        {
            _disallowAdds = true;

            foreach (ProjectDescriptorExt pd in _state.Targets.Values)
                pd.LastScanCheckDate = lastCheckDate;

            _state.saveProjectCheckState();

            _log.Info
                ($"Resolved {ResolvedScanCount} scans to check in " +
                $"{ResolvedProjectCount} projects since {lastCheckDate}.");

            if (_log.IsDebugEnabled)
            {
                foreach (int projectId in _projectCount.Keys)
                    _log.DebugFormat("Project {0} checking {1} scans.", projectId, _projectCount[projectId]);
            }

            return _scans.Values;
        }

    }
}
