Implementation in .NET Core so it can run on Linux or Windows if required.

ConsoleRunner is a test program that executes a single iteration of the process for demo/debug purposes.

A Windows service will be created that executes the iterations based on a configured period.  The service should use the app.config
to allow configuration of the following elements:
* SAST Instance URL, login parameters (username/password and token options), timeout parameters, etc.
* The period, in minutes, between each iteration.  The default config value should be 120 minutes.
* The number of concurrent threads that will run during the transformation.  The default config should be 2.
* The transformed data retention period in days.  The default config value should be 14.
* The location of the file where state persistence is read/stored.
* The location of the root folder where output log files will be written.

The goal is to be able to replace the transformation output with other transformations.  Retrieving the data will be
relatively the same for all versions of SAST, the data output may be defined at install time.

Current thought is to use a simple Log4Net logging facility to write record data as a "log file" for log forwarding via 
the Splunk universal forwarder.  Some examples have been provided in the stubbed out project.


# Requirements as stated in the SOW

Checkmarx will deliver a polling service installed in the Checkmarx hosted environment that will, via available Checkmarx APIs, 
iterate through projects on a single SAST instance.  The polling service will function as follows:
�	It will periodically begin the iteration through existing projects to retrieve and transform scan results to a form easily consumed and queried by Splunk.
�	The iteration will perform to completion upon start; if an iteration in progress has not completed before any subsequent iteration is to begin, subsequent iterations will be skipped until such time as a subsequent iteration schedule occurs when there is no iteration in progress.
�	When hosted on a manager node, the iteration will at maximum consume 2 concurrent threads of execution and be configured with a minimum interval period of 2 hours.
�	Data that will be transformed will be limited to data available at the time the iteration commences.
�	The polling will collect all scans not transformed in prior iterations for all projects and all teams. 
�	Public full and incremental scans will be included in the data transformed and forwarded to Splunk.  Private scans and scan subsets will be excluded.
�	Transformed data will be written locally with the intention of the Splunk Universal Forwarder picking it up and forwarding it to a remote Splunk instance.
�	Local transformed data will be retained for a maximum of 2 weeks before being permanently purged.  (Scan results in the Checkmarx system will not be purged using this same mechanism; the data retention feature will manage the Checkmarx data retention separately.)

The data that is extracted from results and transformed will follow the specification in the document detailing the data requirements for the user story �As a Security Engineer, I want to be able to provide metrics that will give management a high-level view of the security status of AAP applications while being detailed enough for the Engineer�s day to day statistics.�  The data may be modified from the stated requirements to add data elements to each record type.  Some examples of the types of modifications:
�	Metadata such as the �scan type� may be added to some records to allow data to be easily filtered when composing Splunk queries
�	Fields that a redundant with other records may be added to some records to avoid result count limits imposed by Splunk
�	Extra fields will be added in cases where specified record structures don�t have the fields required to unambiguously identify a unique record (e.g. SAST Scan summary does not list Scan Id as a field)
�	Redundant fields or hashed combinations of fields may be added to form the functional equivalent of a composite key as necessary
�	All data records will be delivered in log format with an ISO 8601 datestamp of the time the data record was logged followed by a JSON representation of key/value pairs

Assumptions are as follows:
�	Customer provides support necessary to access their Splunk endpoint.
�	Customer provides the Splunk sourcetypes and any specific parsing parameters other than timestamp recognition used when configuring the universal forwarder.
�	The JSON format will be parsed by Splunk natively; customer configures any specific field extractions for any Splunk sourcetype if required.



# Data Record Specification

Some of the specified data records need to be modified given they don't have all required fields.

## SAST Scan Summary Fields

* Project Name
* Team Name
* Scan Id (added)
* Scan Type (added)
* LOC
* Scan Start
* Scan End
* Preset
* High
* Medium
* Low

## SAST Vulnerability Details

* Project Name
* Team Name
* Scan ID
* Scan Type (added)
* Unique Vulnerability ID
* Status
* State
* Query Name
* Severity
* DeepLink
* File Name
* Line
* Column
* Node ID
* Name
* Type
* Length
* Snippet (must be escaped so it doesn't cause JSON problems)


## OSA Scan Summary

* Project Name
* Team Name (added)
* Scan ID (added)
* Scan Start
* Scan End
* Number of Open Source Detected
* Number of Unrecognized
* High
* Medium
* Low
* Legal High
* Legal Medium
* Legal Low


## OSA Vulnerability Details

* Project Name
* Team Name
* Scan ID 
* CVE Name 
* CVE Score 
* CVE URL 
* Severity 
* CVE Description 
* Recommendation 
* Library Name 
* Library Current Version 
* Library Newest Version 
* Library Current Version Date 
* Library Newest Version Date 
* State 
* Legal License 
* LegalRisk

## Project Information

* Project Name 
* Team Name 
* Preset 
* Last Scan Date 
* Total Scans 
* Policies

## Policy Violations

* Project Name
* Team Name (added)
* Scan ID (added - may not apply if policy violations are not related to a scan)
* Sast Scan Type (added - may not apply)
* Number of policies violated 
* Number of rules violated 
* Names of policies 
* Description of rules 
* Number of occurrences 
* Scan type 
* Rule type

Note that they may not have M&O installed.  Data for scans may also take some time to reach the M&O database after the scan is complete.
