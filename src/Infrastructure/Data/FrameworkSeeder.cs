using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Infrastructure.Data;

/// <summary>
/// Seeds system compliance frameworks and their controls.
/// These are global, tenant-agnostic frameworks that all organizations can use.
/// </summary>
public class FrameworkSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FrameworkSeeder> _logger;

    public FrameworkSeeder(ApplicationDbContext context, ILogger<FrameworkSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedFrameworksAsync()
    {
        if (_context.ComplianceFrameworks.Any(f => f.IsSystemFramework))
        {
            _logger.LogInformation("System frameworks already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding system compliance frameworks...");

        // Seed all frameworks
        await SeedDORAFrameworkAsync();
        await SeedISO27001FrameworkAsync();
        await SeedSOC2FrameworkAsync();
        await SeedGDPRFrameworkAsync();
        await SeedPCIDSSFrameworkAsync();

        await _context.SaveChangesAsync();

        _logger.LogInformation("System frameworks seeded successfully.");
    }

    private async Task SeedDORAFrameworkAsync()
    {
        var dora = new ComplianceFramework
        {
            Id = Guid.NewGuid(),
            Code = "DORA",
            Name = "Digital Operational Resilience Act",
            Description = "EU regulation establishing a comprehensive framework for ICT risk management and operational resilience for financial entities.",
            Version = "2022",
            Category = FrameworkCategory.Financial,
            IssuingAuthority = "European Parliament and Council",
            PublicationDate = new DateTime(2022, 12, 27),
            EffectiveDate = new DateTime(2025, 1, 17),
            IsSystemFramework = true,
            IsActive = true
        };

        // DORA Controls - Key requirements organized by pillars
        var doraControls = new List<ComplianceControl>
        {
            // Pillar 1: ICT Risk Management (Articles 5-16)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-5",
                Title = "Governance and Organization",
                Description = "Financial entities shall have in place an internal governance and control framework ensuring effective and prudent management of ICT risk.",
                ImplementationGuidance = "Establish clear roles and responsibilities for ICT risk management at management body level. Implement oversight mechanisms and reporting lines.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 1,
                IsMandatory = true,
                Tags = "[\"governance\",\"risk-management\",\"organizational\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-6",
                Title = "ICT Risk Management Framework",
                Description = "Financial entities shall have a sound, comprehensive and well-documented ICT risk management framework as part of their overall risk management system.",
                ImplementationGuidance = "Document and implement comprehensive ICT risk management policies, procedures, and protocols. Include risk identification, protection, detection, response and recovery.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 2,
                IsMandatory = true,
                Tags = "[\"risk-framework\",\"documentation\",\"policies\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-8",
                Title = "Identification",
                Description = "Financial entities shall identify, classify and adequately document all ICT-supported business functions, roles and responsibilities, and the information assets supporting them.",
                ImplementationGuidance = "Maintain inventory of ICT assets, business functions, dependencies, and data flows. Classify based on criticality and risk exposure.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 3,
                IsMandatory = true,
                Tags = "[\"asset-management\",\"classification\",\"inventory\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-9",
                Title = "Protection and Prevention",
                Description = "Financial entities shall design, procure and implement ICT security policies, procedures, protocols and tools that aim to ensure the resilience, continuity and availability of ICT systems.",
                ImplementationGuidance = "Implement security controls for network security, access management, encryption, segregation of duties, change management, and patch management.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 4,
                IsMandatory = true,
                Tags = "[\"security\",\"protection\",\"prevention\",\"controls\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-10",
                Title = "Detection",
                Description = "Financial entities shall have mechanisms to promptly detect anomalous activities, including ICT network performance issues and ICT-related incidents.",
                ImplementationGuidance = "Implement continuous monitoring, logging, and alerting systems. Deploy security information and event management (SIEM) tools.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 5,
                IsMandatory = true,
                Tags = "[\"detection\",\"monitoring\",\"siem\",\"alerting\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-11",
                Title = "Response and Recovery",
                Description = "Financial entities shall put in place comprehensive ICT business continuity policy and ICT response and recovery plans.",
                ImplementationGuidance = "Develop and test incident response plans, disaster recovery plans, and business continuity plans. Define recovery time objectives (RTO) and recovery point objectives (RPO).",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 6,
                IsMandatory = true,
                Tags = "[\"incident-response\",\"business-continuity\",\"disaster-recovery\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-13",
                Title = "Communication",
                Description = "Financial entities shall have communication plans enabling responsible disclosure of major ICT-related incidents to clients and counterparts.",
                ImplementationGuidance = "Establish communication protocols and templates for internal and external stakeholder notification during incidents.",
                Category = "ICT Risk Management",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 7,
                IsMandatory = true,
                Tags = "[\"communication\",\"disclosure\",\"incident-reporting\"]"
            },

            // Pillar 2: ICT-Related Incident Management (Articles 17-23)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-17",
                Title = "ICT-Related Incident Management Process",
                Description = "Financial entities shall define, establish and implement an ICT-related incident management process to detect, manage and notify ICT-related incidents.",
                ImplementationGuidance = "Implement incident classification, logging, escalation procedures, and root cause analysis processes.",
                Category = "Incident Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 8,
                IsMandatory = true,
                Tags = "[\"incident-management\",\"incident-classification\",\"escalation\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-19",
                Title = "Incident Reporting to Competent Authorities",
                Description = "Financial entities shall report major ICT-related incidents to the relevant competent authority.",
                ImplementationGuidance = "Establish procedures for timely reporting to supervisory authorities. Initial notification within set timeframes, followed by intermediate and final reports.",
                Category = "Incident Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 9,
                IsMandatory = true,
                Tags = "[\"regulatory-reporting\",\"compliance\",\"incident-notification\"]"
            },

            // Pillar 3: Digital Operational Resilience Testing (Articles 24-27)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-24",
                Title = "General Requirements for Testing",
                Description = "Financial entities shall establish, maintain and review a sound and comprehensive digital operational resilience testing programme.",
                ImplementationGuidance = "Develop testing program including vulnerability assessments, security assessments, scenario-based testing, and penetration testing.",
                Category = "Resilience Testing",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 10,
                IsMandatory = true,
                Tags = "[\"testing\",\"penetration-testing\",\"vulnerability-assessment\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-26",
                Title = "Threat-Led Penetration Testing (TLPT)",
                Description = "Significant financial entities shall carry out threat-led penetration testing on critical or important functions and services at least every three years.",
                ImplementationGuidance = "Engage qualified external testers to conduct advanced testing simulating real-world attack scenarios. Cover people, processes, and technologies.",
                Category = "Resilience Testing",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 11,
                IsMandatory = true,
                Tags = "[\"tlpt\",\"penetration-testing\",\"red-team\"]"
            },

            // Pillar 4: Third-Party Risk Management (Articles 28-30)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-28",
                Title = "Third-Party Risk Management",
                Description = "Financial entities shall manage ICT third-party risk as an integral component of ICT risk within their ICT risk management framework.",
                ImplementationGuidance = "Implement due diligence, risk assessment, and ongoing monitoring of ICT third-party service providers. Maintain a register of all third-party arrangements.",
                Category = "Third-Party Risk",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 12,
                IsMandatory = true,
                Tags = "[\"third-party\",\"vendor-management\",\"supply-chain\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-30",
                Title = "Contractual Arrangements for Third-Party Providers",
                Description = "Contractual arrangements on the use of ICT services shall include at minimum service level agreements, performance targets, liability provisions, and termination rights.",
                ImplementationGuidance = "Ensure contracts include provisions for access rights, audit rights, data protection, incident notification, exit strategies, and subcontracting limitations.",
                Category = "Third-Party Risk",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 13,
                IsMandatory = true,
                Tags = "[\"contracts\",\"sla\",\"legal\"]"
            },

            // Pillar 5: Information Sharing (Articles 45)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = dora.Id,
                ControlCode = "ART-45",
                Title = "Information Sharing Arrangements",
                Description = "Financial entities may exchange cyber threat information and intelligence, including indicators of compromise, tactics, techniques, and procedures.",
                ImplementationGuidance = "Participate in information sharing arrangements with other financial entities and cybersecurity centers. Implement secure channels for threat intelligence exchange.",
                Category = "Information Sharing",
                DefaultRiskLevel = RiskLevel.Low,
                SortOrder = 14,
                IsMandatory = false,
                Tags = "[\"threat-intelligence\",\"information-sharing\",\"collaboration\"]"
            }
        };

        dora.Controls = doraControls;
        _context.ComplianceFrameworks.Add(dora);
    }

    private async Task SeedISO27001FrameworkAsync()
    {
        var iso27001 = new ComplianceFramework
        {
            Id = Guid.NewGuid(),
            Code = "ISO27001",
            Name = "ISO/IEC 27001:2022",
            Description = "International standard for information security management systems (ISMS). Provides requirements for establishing, implementing, maintaining and continually improving an ISMS.",
            Version = "2022",
            Category = FrameworkCategory.Security,
            IssuingAuthority = "ISO/IEC",
            PublicationDate = new DateTime(2022, 10, 25),
            EffectiveDate = new DateTime(2022, 10, 25),
            IsSystemFramework = true,
            IsActive = true
        };

        // ISO 27001:2022 Controls - Annex A
        var iso27001Controls = new List<ComplianceControl>
        {
            // 5. Organizational Controls
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.5.1",
                Title = "Policies for Information Security",
                Description = "Information security policy and topic-specific policies shall be defined, approved by management, published, communicated to and acknowledged by relevant personnel and relevant interested parties, and reviewed at planned intervals.",
                ImplementationGuidance = "Develop comprehensive information security policies covering scope, objectives, responsibilities, and compliance requirements. Ensure management approval and regular reviews.",
                Category = "Organizational Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 1,
                IsMandatory = true,
                Tags = "[\"policy\",\"governance\",\"documentation\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.5.2",
                Title = "Information Security Roles and Responsibilities",
                Description = "Information security roles and responsibilities shall be defined and allocated according to the organization's needs.",
                ImplementationGuidance = "Clearly define and document security roles (CISO, security officers, data owners, etc.). Ensure segregation of duties and accountability.",
                Category = "Organizational Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 2,
                IsMandatory = true,
                Tags = "[\"roles\",\"responsibilities\",\"governance\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.5.7",
                Title = "Threat Intelligence",
                Description = "Information relating to information security threats shall be collected and analyzed to produce threat intelligence.",
                ImplementationGuidance = "Establish processes to gather, analyze, and act on threat intelligence from internal and external sources.",
                Category = "Organizational Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 3,
                IsMandatory = true,
                Tags = "[\"threat-intelligence\",\"monitoring\",\"analysis\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.5.10",
                Title = "Acceptable Use of Information and Other Associated Assets",
                Description = "Rules for the acceptable use and procedures for handling information and other associated assets shall be identified, documented and implemented.",
                ImplementationGuidance = "Define acceptable use policies covering data handling, device usage, internet access, and email communications.",
                Category = "Organizational Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 4,
                IsMandatory = true,
                Tags = "[\"acceptable-use\",\"policy\",\"user-behavior\"]"
            },

            // 6. People Controls
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.6.1",
                Title = "Screening",
                Description = "Background verification checks on all candidates for employment shall be carried out prior to joining the organization and on an ongoing basis.",
                ImplementationGuidance = "Implement background check processes appropriate to business requirements, classification of information, and perceived risks.",
                Category = "People Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 5,
                IsMandatory = true,
                Tags = "[\"hr\",\"background-checks\",\"screening\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.6.2",
                Title = "Terms and Conditions of Employment",
                Description = "The employment contractual agreements shall state the personnel's and the organization's responsibilities for information security.",
                ImplementationGuidance = "Include security responsibilities, confidentiality agreements, and acceptable use terms in employment contracts.",
                Category = "People Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 6,
                IsMandatory = true,
                Tags = "[\"hr\",\"contracts\",\"legal\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.6.3",
                Title = "Information Security Awareness, Education and Training",
                Description = "Personnel of the organization and relevant interested parties shall receive appropriate information security awareness, education and training.",
                ImplementationGuidance = "Deliver regular security awareness training covering policies, threat landscape, and secure behaviors. Track completion and effectiveness.",
                Category = "People Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 7,
                IsMandatory = true,
                Tags = "[\"training\",\"awareness\",\"education\"]"
            },

            // 7. Physical Controls
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.7.2",
                Title = "Physical Entry",
                Description = "Secure areas shall be protected by appropriate entry controls and access points.",
                ImplementationGuidance = "Implement physical access controls including badge readers, biometrics, visitor logs, and security guards where appropriate.",
                Category = "Physical Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 8,
                IsMandatory = true,
                Tags = "[\"physical-security\",\"access-control\",\"facilities\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.7.4",
                Title = "Physical Security Monitoring",
                Description = "Premises shall be continuously monitored for unauthorized physical access.",
                ImplementationGuidance = "Deploy CCTV, alarm systems, and security patrols. Review and retain security footage per policy.",
                Category = "Physical Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 9,
                IsMandatory = true,
                Tags = "[\"physical-security\",\"monitoring\",\"cctv\"]"
            },

            // 8. Technological Controls
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.2",
                Title = "Privileged Access Rights",
                Description = "The allocation and use of privileged access rights shall be restricted and managed.",
                ImplementationGuidance = "Implement least privilege principle, regular access reviews, and privileged access management (PAM) solutions.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 10,
                IsMandatory = true,
                Tags = "[\"access-control\",\"privileged-access\",\"pam\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.3",
                Title = "Information Access Restriction",
                Description = "Access to information and other associated assets shall be restricted in accordance with the established topic-specific policy on access control.",
                ImplementationGuidance = "Implement role-based access control (RBAC), need-to-know principles, and data classification-based access restrictions.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 11,
                IsMandatory = true,
                Tags = "[\"access-control\",\"rbac\",\"authorization\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.5",
                Title = "Secure Authentication",
                Description = "Secure authentication technologies and procedures shall be implemented based on information access restrictions and the topic-specific policy on access control.",
                ImplementationGuidance = "Implement multi-factor authentication (MFA), strong password policies, and secure authentication protocols.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 12,
                IsMandatory = true,
                Tags = "[\"authentication\",\"mfa\",\"passwords\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.10",
                Title = "Information Deletion",
                Description = "Information stored in information systems, devices or in any other storage media shall be deleted when no longer required.",
                ImplementationGuidance = "Implement secure deletion procedures and data retention policies. Use cryptographic erasure where appropriate.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 13,
                IsMandatory = true,
                Tags = "[\"data-deletion\",\"retention\",\"disposal\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.11",
                Title = "Data Masking",
                Description = "Data masking shall be used in accordance with the organization's topic-specific policy on access control and other related topic-specific policies.",
                ImplementationGuidance = "Implement data masking for non-production environments and when sharing data with third parties.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 14,
                IsMandatory = true,
                Tags = "[\"data-masking\",\"privacy\",\"testing\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.16",
                Title = "Monitoring Activities",
                Description = "Networks, systems and applications shall be monitored for anomalous behavior and appropriate actions taken to evaluate potential information security incidents.",
                ImplementationGuidance = "Deploy SIEM, intrusion detection systems (IDS), and log monitoring. Establish baselines and alerting thresholds.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 15,
                IsMandatory = true,
                Tags = "[\"monitoring\",\"siem\",\"detection\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.23",
                Title = "Web Filtering",
                Description = "Access to external websites shall be managed to reduce exposure to malicious content.",
                ImplementationGuidance = "Implement web filtering solutions to block malicious, inappropriate, or risky websites.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 16,
                IsMandatory = true,
                Tags = "[\"web-filtering\",\"malware-protection\",\"network-security\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.24",
                Title = "Use of Cryptography",
                Description = "Rules for the effective use of cryptography, including cryptographic key management, shall be defined and implemented.",
                ImplementationGuidance = "Implement encryption for data at rest and in transit. Establish key management lifecycle procedures.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 17,
                IsMandatory = true,
                Tags = "[\"cryptography\",\"encryption\",\"key-management\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.28",
                Title = "Secure Coding",
                Description = "Secure coding principles shall be applied to software development.",
                ImplementationGuidance = "Adopt secure coding standards (OWASP), conduct code reviews, and use static/dynamic code analysis tools.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 18,
                IsMandatory = true,
                Tags = "[\"secure-coding\",\"development\",\"owasp\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.31",
                Title = "Separation of Development, Test and Production Environments",
                Description = "Development, testing and production environments shall be separated and secured.",
                ImplementationGuidance = "Maintain separate environments with different access controls. Prevent production data in lower environments.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 19,
                IsMandatory = true,
                Tags = "[\"environment-separation\",\"sdlc\",\"development\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = iso27001.Id,
                ControlCode = "A.8.34",
                Title = "Protection of Information Systems During Audit Testing",
                Description = "Tests and other assurance activities involving assessment of operational systems shall be planned and agreed between the tester and appropriate management.",
                ImplementationGuidance = "Require formal approval for penetration testing and security assessments. Use separate test environments where possible.",
                Category = "Technological Controls",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 20,
                IsMandatory = true,
                Tags = "[\"audit\",\"testing\",\"security-assessment\"]"
            }
        };

        iso27001.Controls = iso27001Controls;
        _context.ComplianceFrameworks.Add(iso27001);
    }

    private async Task SeedSOC2FrameworkAsync()
    {
        var soc2 = new ComplianceFramework
        {
            Id = Guid.NewGuid(),
            Code = "SOC2",
            Name = "SOC 2 Type II",
            Description = "Service Organization Control 2 - Trust Services Criteria for security, availability, processing integrity, confidentiality, and privacy of customer data in service organizations.",
            Version = "2017",
            Category = FrameworkCategory.Financial,
            IssuingAuthority = "AICPA",
            PublicationDate = new DateTime(2017, 12, 15),
            EffectiveDate = new DateTime(2017, 12, 15),
            IsSystemFramework = true,
            IsActive = true
        };

        // SOC 2 Trust Services Criteria
        var soc2Controls = new List<ComplianceControl>
        {
            // Common Criteria (CC) - Apply to all Trust Services Categories
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC1.1",
                Title = "Control Environment - Integrity and Ethical Values",
                Description = "The entity demonstrates a commitment to integrity and ethical values.",
                ImplementationGuidance = "Establish and communicate code of conduct, ethics policies, and behavioral standards. Monitor compliance and address violations.",
                Category = "Common Criteria - Control Environment",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 1,
                IsMandatory = true,
                Tags = "[\"governance\",\"ethics\",\"culture\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC1.2",
                Title = "Board Independence and Oversight",
                Description = "The board of directors demonstrates independence from management and exercises oversight of the system.",
                ImplementationGuidance = "Ensure board has appropriate expertise, receives regular security reports, and provides governance oversight.",
                Category = "Common Criteria - Control Environment",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 2,
                IsMandatory = true,
                Tags = "[\"governance\",\"board\",\"oversight\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC2.1",
                Title = "Communication and Information - Internal Communication",
                Description = "The entity obtains or generates and uses relevant, quality information to support the functioning of internal control.",
                ImplementationGuidance = "Establish communication channels for security-related information. Ensure timely and accurate information flow.",
                Category = "Common Criteria - Communication",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 3,
                IsMandatory = true,
                Tags = "[\"communication\",\"information-flow\",\"internal\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC3.1",
                Title = "Risk Assessment Process",
                Description = "The entity specifies objectives with sufficient clarity to enable the identification and assessment of risks.",
                ImplementationGuidance = "Define clear security objectives aligned with business goals. Conduct regular risk assessments.",
                Category = "Common Criteria - Risk Assessment",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 4,
                IsMandatory = true,
                Tags = "[\"risk-assessment\",\"objectives\",\"planning\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC3.4",
                Title = "Fraud Risk Assessment",
                Description = "The entity identifies and assesses risks associated with fraud.",
                ImplementationGuidance = "Conduct fraud risk assessments considering internal and external threats. Implement anti-fraud controls.",
                Category = "Common Criteria - Risk Assessment",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 5,
                IsMandatory = true,
                Tags = "[\"fraud\",\"risk-assessment\",\"threat-analysis\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC5.1",
                Title = "Control Activities - Selection and Development",
                Description = "The entity selects and develops control activities that contribute to the mitigation of risks.",
                ImplementationGuidance = "Design and implement controls addressing identified risks. Ensure controls are appropriate and effective.",
                Category = "Common Criteria - Control Activities",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 6,
                IsMandatory = true,
                Tags = "[\"controls\",\"risk-mitigation\",\"design\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC6.1",
                Title = "Logical and Physical Access - Boundary Protection",
                Description = "The entity implements logical access security measures to protect against threats from sources outside its system boundaries.",
                ImplementationGuidance = "Deploy firewalls, intrusion prevention systems, and network segmentation. Monitor network boundaries.",
                Category = "Common Criteria - Logical Access",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 7,
                IsMandatory = true,
                Tags = "[\"network-security\",\"firewall\",\"perimeter\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC6.2",
                Title = "User Identification and Authentication",
                Description = "Prior to issuing system credentials and granting system access, the entity registers and authorizes new internal and external users.",
                ImplementationGuidance = "Implement user provisioning processes, identity verification, and approval workflows for access requests.",
                Category = "Common Criteria - Logical Access",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 8,
                IsMandatory = true,
                Tags = "[\"access-control\",\"authentication\",\"provisioning\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC6.3",
                Title = "Multi-Factor Authentication",
                Description = "The entity requires users to authenticate using multiple authentication factors.",
                ImplementationGuidance = "Implement MFA for all user access, especially privileged accounts and remote access.",
                Category = "Common Criteria - Logical Access",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 9,
                IsMandatory = true,
                Tags = "[\"mfa\",\"authentication\",\"security\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC6.6",
                Title = "Access Removal",
                Description = "The entity removes access when user access is no longer appropriate.",
                ImplementationGuidance = "Implement automated deprovisioning processes. Conduct regular access reviews and remove unnecessary access.",
                Category = "Common Criteria - Logical Access",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 10,
                IsMandatory = true,
                Tags = "[\"access-control\",\"deprovisioning\",\"user-lifecycle\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC7.1",
                Title = "System Operations - Detection of Security Events",
                Description = "To meet its objectives, the entity uses detection and monitoring procedures to identify anomalies.",
                ImplementationGuidance = "Deploy monitoring tools, configure alerting, and establish incident detection procedures.",
                Category = "Common Criteria - System Operations",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 11,
                IsMandatory = true,
                Tags = "[\"monitoring\",\"detection\",\"siem\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC7.2",
                Title = "Incident Response",
                Description = "The entity responds to identified security incidents by executing a defined incident response program.",
                ImplementationGuidance = "Develop incident response plan, establish response team, and conduct regular tabletop exercises.",
                Category = "Common Criteria - System Operations",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 12,
                IsMandatory = true,
                Tags = "[\"incident-response\",\"security-operations\",\"playbooks\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC7.3",
                Title = "Vulnerability Management",
                Description = "The entity identifies, develops, and carries out vulnerability management activities.",
                ImplementationGuidance = "Conduct regular vulnerability scans, prioritize remediation, and track patching cycles.",
                Category = "Common Criteria - System Operations",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 13,
                IsMandatory = true,
                Tags = "[\"vulnerability-management\",\"patching\",\"scanning\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "CC8.1",
                Title = "Change Management",
                Description = "The entity authorizes, designs, develops, configures, documents, tests, approves, and implements changes to infrastructure, data, software, and procedures.",
                ImplementationGuidance = "Implement formal change management process with approval workflows, testing, and rollback procedures.",
                Category = "Common Criteria - Change Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 14,
                IsMandatory = true,
                Tags = "[\"change-management\",\"sdlc\",\"approval\"]"
            },

            // Additional Trust Services Criteria - Availability (A)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "A1.1",
                Title = "Availability Commitments",
                Description = "The entity maintains, monitors, and evaluates system availability based on defined availability commitments.",
                ImplementationGuidance = "Define SLAs, monitor uptime, and implement high availability architecture.",
                Category = "Availability",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 15,
                IsMandatory = true,
                Tags = "[\"availability\",\"sla\",\"uptime\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "A1.2",
                Title = "Environmental Protections",
                Description = "The entity authorizes, designs, develops or acquires, implements, operates, approves, maintains, and monitors environmental protections.",
                ImplementationGuidance = "Implement redundant power, cooling, fire suppression, and environmental monitoring systems.",
                Category = "Availability",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 16,
                IsMandatory = true,
                Tags = "[\"datacenter\",\"physical\",\"environmental\"]"
            },

            // Confidentiality (C)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "C1.1",
                Title = "Confidentiality Commitments",
                Description = "The entity identifies and maintains confidential information to meet commitments and system requirements.",
                ImplementationGuidance = "Classify confidential data, implement encryption, and restrict access to authorized users only.",
                Category = "Confidentiality",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 17,
                IsMandatory = true,
                Tags = "[\"confidentiality\",\"data-classification\",\"encryption\"]"
            },

            // Privacy (P)
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "P1.1",
                Title = "Privacy Notice",
                Description = "The entity provides notice to data subjects about its privacy practices.",
                ImplementationGuidance = "Publish privacy policy, provide notice at collection, and update when practices change.",
                Category = "Privacy",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 18,
                IsMandatory = true,
                Tags = "[\"privacy\",\"notice\",\"transparency\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = soc2.Id,
                ControlCode = "P4.1",
                Title = "Data Subject Rights",
                Description = "The entity grants data subjects the ability to access and correct their personal information.",
                ImplementationGuidance = "Implement processes for data subject access requests (DSAR), correction, and deletion.",
                Category = "Privacy",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 19,
                IsMandatory = true,
                Tags = "[\"privacy\",\"data-subject-rights\",\"dsar\"]"
            }
        };

        soc2.Controls = soc2Controls;
        _context.ComplianceFrameworks.Add(soc2);
    }

    private async Task SeedGDPRFrameworkAsync()
    {
        var gdpr = new ComplianceFramework
        {
            Id = Guid.NewGuid(),
            Code = "GDPR",
            Name = "General Data Protection Regulation",
            Description = "EU regulation on data protection and privacy for all individuals within the European Union and the European Economic Area.",
            Version = "2016",
            Category = FrameworkCategory.Privacy,
            IssuingAuthority = "European Parliament and Council",
            PublicationDate = new DateTime(2016, 4, 27),
            EffectiveDate = new DateTime(2018, 5, 25),
            IsSystemFramework = true,
            IsActive = true
        };

        // GDPR Key Articles
        var gdprControls = new List<ComplianceControl>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-5",
                Title = "Principles Relating to Processing of Personal Data",
                Description = "Personal data must be processed lawfully, fairly and transparently; collected for specified, explicit and legitimate purposes; adequate, relevant and limited; accurate and up to date; kept no longer than necessary; processed securely.",
                ImplementationGuidance = "Document lawful basis for processing. Implement data minimization. Establish retention schedules. Ensure accuracy through regular reviews.",
                Category = "Core Principles",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 1,
                IsMandatory = true,
                Tags = "[\"principles\",\"lawfulness\",\"transparency\",\"purpose-limitation\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-6",
                Title = "Lawfulness of Processing",
                Description = "Processing is lawful only if based on: consent, contract, legal obligation, vital interests, public task, or legitimate interests.",
                ImplementationGuidance = "Identify and document lawful basis for each processing activity. Obtain valid consent where required.",
                Category = "Lawful Processing",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 2,
                IsMandatory = true,
                Tags = "[\"lawful-basis\",\"consent\",\"legal-grounds\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-7",
                Title = "Conditions for Consent",
                Description = "Consent must be freely given, specific, informed and unambiguous. Must be as easy to withdraw as to give.",
                ImplementationGuidance = "Implement consent management platform. Use clear language, separate consent from other terms, and provide easy withdrawal mechanism.",
                Category = "Lawful Processing",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 3,
                IsMandatory = true,
                Tags = "[\"consent\",\"consent-management\",\"withdrawal\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-12",
                Title = "Transparent Information and Communication",
                Description = "Provide information to data subjects in concise, transparent, intelligible and easily accessible form, using clear and plain language.",
                ImplementationGuidance = "Create layered privacy notices. Use simple language. Make notices easily accessible and prominent.",
                Category = "Transparency",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 4,
                IsMandatory = true,
                Tags = "[\"transparency\",\"privacy-notice\",\"communication\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-13",
                Title = "Information to be Provided - Direct Collection",
                Description = "When collecting data directly from data subject, provide: controller identity, DPO contact, purposes, lawful basis, recipients, retention period, data subject rights.",
                ImplementationGuidance = "Include all required elements in privacy notice at point of collection. Update notices when processing changes.",
                Category = "Transparency",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 5,
                IsMandatory = true,
                Tags = "[\"privacy-notice\",\"data-collection\",\"transparency\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-15",
                Title = "Right of Access by Data Subject",
                Description = "Data subject has right to obtain confirmation of processing, access to data, and information about processing.",
                ImplementationGuidance = "Implement DSAR portal or process. Respond within one month. Provide data in structured, commonly used format.",
                Category = "Data Subject Rights",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 6,
                IsMandatory = true,
                Tags = "[\"dsar\",\"access-rights\",\"subject-rights\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-16",
                Title = "Right to Rectification",
                Description = "Data subject has right to obtain rectification of inaccurate personal data and completion of incomplete data.",
                ImplementationGuidance = "Provide mechanism for data subjects to request corrections. Verify and update data. Notify recipients of corrections.",
                Category = "Data Subject Rights",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 7,
                IsMandatory = true,
                Tags = "[\"rectification\",\"data-accuracy\",\"subject-rights\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-17",
                Title = "Right to Erasure ('Right to be Forgotten')",
                Description = "Data subject has right to obtain erasure of personal data in certain circumstances.",
                ImplementationGuidance = "Implement data deletion workflows. Assess erasure requests against legal bases and exceptions. Document decisions.",
                Category = "Data Subject Rights",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 8,
                IsMandatory = true,
                Tags = "[\"erasure\",\"right-to-be-forgotten\",\"deletion\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-20",
                Title = "Right to Data Portability",
                Description = "Data subject has right to receive personal data in structured, commonly used, machine-readable format and transmit to another controller.",
                ImplementationGuidance = "Enable data export in JSON, CSV, or XML formats. Automate portability where feasible.",
                Category = "Data Subject Rights",
                DefaultRiskLevel = RiskLevel.Medium,
                SortOrder = 9,
                IsMandatory = true,
                Tags = "[\"portability\",\"data-export\",\"interoperability\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-25",
                Title = "Data Protection by Design and by Default",
                Description = "Implement appropriate technical and organizational measures to ensure data protection principles and safeguard data subject rights from the design stage.",
                ImplementationGuidance = "Conduct Privacy Impact Assessments (PIA). Build privacy into system design. Default to highest privacy settings.",
                Category = "Privacy by Design",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 10,
                IsMandatory = true,
                Tags = "[\"privacy-by-design\",\"privacy-by-default\",\"engineering\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-28",
                Title = "Processor Obligations",
                Description = "Use only processors providing sufficient guarantees. Processor must process only on documented instructions and comply with controller obligations.",
                ImplementationGuidance = "Execute Data Processing Agreements (DPA) with all processors. Conduct vendor due diligence. Audit processor compliance.",
                Category = "Third-Party Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 11,
                IsMandatory = true,
                Tags = "[\"processors\",\"dpa\",\"vendor-management\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-30",
                Title = "Records of Processing Activities",
                Description = "Maintain records of all processing activities including: purposes, categories of data subjects and data, recipients, transfers, retention periods, security measures.",
                ImplementationGuidance = "Create and maintain Register of Processing Activities (RoPA). Review and update regularly. Make available to supervisory authority.",
                Category = "Accountability",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 12,
                IsMandatory = true,
                Tags = "[\"ropa\",\"record-keeping\",\"documentation\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-32",
                Title = "Security of Processing",
                Description = "Implement appropriate technical and organizational measures to ensure security appropriate to risk: pseudonymization, encryption, confidentiality, integrity, availability, resilience.",
                ImplementationGuidance = "Conduct risk assessments. Implement encryption, access controls, monitoring. Test and evaluate effectiveness regularly.",
                Category = "Security",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 13,
                IsMandatory = true,
                Tags = "[\"security\",\"encryption\",\"risk-management\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-33",
                Title = "Notification of Personal Data Breach to Supervisory Authority",
                Description = "Notify supervisory authority of data breach within 72 hours of becoming aware, unless unlikely to result in risk to rights and freedoms.",
                ImplementationGuidance = "Establish breach detection and notification procedures. Create notification templates. Maintain breach register.",
                Category = "Breach Management",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 14,
                IsMandatory = true,
                Tags = "[\"breach-notification\",\"incident-response\",\"reporting\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-34",
                Title = "Communication of Personal Data Breach to Data Subject",
                Description = "When breach likely to result in high risk to rights and freedoms, communicate to affected data subjects without undue delay.",
                ImplementationGuidance = "Define high-risk breach criteria. Prepare communication templates. Establish notification channels and workflows.",
                Category = "Breach Management",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 15,
                IsMandatory = true,
                Tags = "[\"breach-notification\",\"data-subject-communication\",\"transparency\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-35",
                Title = "Data Protection Impact Assessment",
                Description = "Conduct DPIA when processing is likely to result in high risk to rights and freedoms, especially when using new technologies or large-scale processing.",
                ImplementationGuidance = "Develop DPIA framework and templates. Conduct DPIAs for high-risk processing. Consult DPO and, when required, supervisory authority.",
                Category = "Privacy Impact Assessment",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 16,
                IsMandatory = true,
                Tags = "[\"dpia\",\"risk-assessment\",\"privacy-impact\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-37",
                Title = "Designation of Data Protection Officer",
                Description = "Appoint DPO if: public authority, core activities require regular systematic monitoring on large scale, or core activities involve large-scale processing of sensitive data.",
                ImplementationGuidance = "Assess DPO requirement. Appoint qualified DPO with necessary resources and independence. Publish DPO contact details.",
                Category = "Governance",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 17,
                IsMandatory = true,
                Tags = "[\"dpo\",\"governance\",\"oversight\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-44",
                Title = "General Principle for International Transfers",
                Description = "Transfer of personal data to third countries only if controller and processor comply with GDPR conditions for transfers.",
                ImplementationGuidance = "Map international data flows. Use adequacy decisions, Standard Contractual Clauses (SCCs), or Binding Corporate Rules (BCRs).",
                Category = "International Transfers",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 18,
                IsMandatory = true,
                Tags = "[\"data-transfers\",\"international\",\"cross-border\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = gdpr.Id,
                ControlCode = "ART-46",
                Title = "Transfers Subject to Appropriate Safeguards",
                Description = "Transfer based on: Standard Contractual Clauses, Binding Corporate Rules, approved codes of conduct, or approved certification mechanisms.",
                ImplementationGuidance = "Execute SCCs with international vendors. Conduct Transfer Impact Assessments (TIA). Document and review transfer mechanisms.",
                Category = "International Transfers",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 19,
                IsMandatory = true,
                Tags = "[\"sccs\",\"safeguards\",\"data-transfers\"]"
            }
        };

        gdpr.Controls = gdprControls;
        _context.ComplianceFrameworks.Add(gdpr);
    }

    private async Task SeedPCIDSSFrameworkAsync()
    {
        var pcidss = new ComplianceFramework
        {
            Id = Guid.NewGuid(),
            Code = "PCIDSS",
            Name = "PCI DSS 4.0",
            Description = "Payment Card Industry Data Security Standard - Security standards for organizations that handle branded credit cards from major card schemes.",
            Version = "4.0",
            Category = FrameworkCategory.Financial,
            IssuingAuthority = "PCI Security Standards Council",
            PublicationDate = new DateTime(2022, 3, 31),
            EffectiveDate = new DateTime(2024, 3, 31),
            IsSystemFramework = true,
            IsActive = true
        };

        // PCI DSS 4.0 Requirements
        var pcidssControls = new List<ComplianceControl>
        {
            // Goal 1: Build and Maintain a Secure Network and Systems
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-1",
                Title = "Install and Maintain Network Security Controls",
                Description = "Network security controls (NSCs) such as firewalls and other network security technologies are network policy enforcement points that restrict network traffic between untrusted and trusted networks.",
                ImplementationGuidance = "Deploy firewalls at all network boundaries. Document and review firewall rules. Segment cardholder data environment (CDE) from other networks.",
                Category = "Network Security",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 1,
                IsMandatory = true,
                Tags = "[\"firewall\",\"network-security\",\"segmentation\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-2",
                Title = "Apply Secure Configurations to All System Components",
                Description = "Malicious individuals, both external and internal, often use default passwords and other vendor default settings to compromise systems.",
                ImplementationGuidance = "Change all vendor defaults. Remove unnecessary services and accounts. Implement secure configuration standards (CIS benchmarks).",
                Category = "Configuration Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 2,
                IsMandatory = true,
                Tags = "[\"hardening\",\"configuration\",\"defaults\"]"
            },

            // Goal 2: Protect Account Data
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-3",
                Title = "Protect Stored Account Data",
                Description = "Protection methods such as encryption, truncation, masking, and hashing are critical components of account data protection.",
                ImplementationGuidance = "Minimize data storage. Encrypt primary account numbers (PAN). Never store sensitive authentication data (CAV2, PIN, etc.) after authorization.",
                Category = "Data Protection",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 3,
                IsMandatory = true,
                Tags = "[\"encryption\",\"data-protection\",\"cardholder-data\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-4",
                Title = "Protect Cardholder Data with Strong Cryptography During Transmission",
                Description = "Sensitive information must be encrypted during transmission over networks that are easily accessed by malicious individuals.",
                ImplementationGuidance = "Use strong cryptography (TLS 1.2+) for all transmission of cardholder data. Never send unencrypted PANs via email or messaging.",
                Category = "Data Protection",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 4,
                IsMandatory = true,
                Tags = "[\"encryption\",\"tls\",\"transmission-security\"]"
            },

            // Goal 3: Maintain a Vulnerability Management Program
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-5",
                Title = "Protect All Systems and Networks from Malicious Software",
                Description = "Malicious software (malware) enters the network during many business-approved activities including employee email and internet usage.",
                ImplementationGuidance = "Deploy anti-malware solutions on all systems. Keep signatures updated. Perform regular scans. Enable automatic updates.",
                Category = "Vulnerability Management",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 5,
                IsMandatory = true,
                Tags = "[\"anti-malware\",\"antivirus\",\"endpoint-protection\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-6",
                Title = "Develop and Maintain Secure Systems and Software",
                Description = "Security vulnerabilities in systems and software may allow criminals to access PAN and other account data.",
                ImplementationGuidance = "Establish secure development lifecycle. Conduct code reviews and security testing. Apply security patches within defined timeframes.",
                Category = "Vulnerability Management",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 6,
                IsMandatory = true,
                Tags = "[\"sdlc\",\"patching\",\"secure-coding\"]"
            },

            // Goal 4: Implement Strong Access Control Measures
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-7",
                Title = "Restrict Access to System Components and Cardholder Data by Business Need to Know",
                Description = "Critical data and systems are protected by limiting access to only those individuals whose jobs require such access.",
                ImplementationGuidance = "Implement role-based access control (RBAC). Define and enforce least privilege. Document and review access rights regularly.",
                Category = "Access Control",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 7,
                IsMandatory = true,
                Tags = "[\"access-control\",\"rbac\",\"least-privilege\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-8",
                Title = "Identify Users and Authenticate Access to System Components",
                Description = "Assigning a unique identification (ID) to each person with access ensures accountability and traceability.",
                ImplementationGuidance = "Assign unique IDs to all users. Implement strong authentication (MFA). Enforce password complexity and change requirements.",
                Category = "Access Control",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 8,
                IsMandatory = true,
                Tags = "[\"authentication\",\"mfa\",\"identity\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-9",
                Title = "Restrict Physical Access to Cardholder Data",
                Description = "Physical access to systems or data provides the opportunity to access devices or data and remove systems or hardcopies.",
                ImplementationGuidance = "Implement physical access controls (badges, biometrics). Maintain visitor logs. Monitor access to sensitive areas. Secure media.",
                Category = "Physical Security",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 9,
                IsMandatory = true,
                Tags = "[\"physical-security\",\"access-control\",\"datacenter\"]"
            },

            // Goal 5: Regularly Monitor and Test Networks
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-10",
                Title = "Log and Monitor All Access to System Components and Cardholder Data",
                Description = "Logging mechanisms and the ability to track user activities are critical in preventing, detecting, or minimizing the impact of a data compromise.",
                ImplementationGuidance = "Enable comprehensive logging on all systems. Protect log integrity. Review logs daily. Retain logs for at least one year.",
                Category = "Monitoring",
                DefaultRiskLevel = RiskLevel.Critical,
                SortOrder = 10,
                IsMandatory = true,
                Tags = "[\"logging\",\"monitoring\",\"audit-trails\"]"
            },
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-11",
                Title = "Test Security of Systems and Networks Regularly",
                Description = "Vulnerabilities are being discovered continually by malicious individuals and researchers, and being introduced by new software.",
                ImplementationGuidance = "Conduct quarterly vulnerability scans. Perform annual penetration testing. Deploy intrusion detection/prevention systems.",
                Category = "Testing",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 11,
                IsMandatory = true,
                Tags = "[\"vulnerability-scanning\",\"penetration-testing\",\"ids-ips\"]"
            },

            // Goal 6: Maintain an Information Security Policy
            new()
            {
                Id = Guid.NewGuid(),
                FrameworkId = pcidss.Id,
                ControlCode = "REQ-12",
                Title = "Support Information Security with Organizational Policies and Programs",
                Description = "The organization's overall information security policy sets the tone for the whole entity and informs personnel what is expected of them.",
                ImplementationGuidance = "Establish comprehensive security policies. Conduct annual risk assessments. Provide security awareness training. Implement incident response plan.",
                Category = "Policy and Governance",
                DefaultRiskLevel = RiskLevel.High,
                SortOrder = 12,
                IsMandatory = true,
                Tags = "[\"policy\",\"governance\",\"risk-assessment\",\"training\"]"
            }
        };

        pcidss.Controls = pcidssControls;
        _context.ComplianceFrameworks.Add(pcidss);
    }
}
