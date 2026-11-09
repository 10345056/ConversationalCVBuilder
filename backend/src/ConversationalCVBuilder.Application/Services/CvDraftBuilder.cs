using System.Text.RegularExpressions;
using ConversationalCVBuilder.Application.Contracts;

namespace ConversationalCVBuilder.Application.Services;

public static partial class CvDraftBuilder
{
    private static readonly (string Role, string[] Terms, string[] Skills, string[] Responsibilities, string[] Achievements)[] RoleProfiles =
    [
        ("Performance Tester",
            ["performance tester", "performance testing", "performance engineer", "jmeter", "loadrunner"],
            ["LoadRunner", "JMeter", "Dynatrace", "Azure Monitor", "Splunk", "Performance Engineering", "API Testing", "SQL", "Azure DevOps", "Git"],
            ["Conduct load, stress, endurance, and spike testing.", "Develop scripts using JMeter and LoadRunner.", "Analyze bottlenecks using Dynatrace and Splunk.", "Execute workload modelling and capacity planning.", "Create performance certification reports."],
            ["Reduce response time through performance analysis.", "Automate performance testing activities.", "Support successful production releases."]),
        ("Automation Tester",
            ["automation tester", "test automation", "automation testing", "selenium", "cypress"],
            ["Selenium", "Cypress", "Playwright", "API Testing", "JMeter", "SQL", "Git", "Azure DevOps"],
            ["Design and maintain automated test cases for agreed requirements.", "Review test results and report reproducible defects.", "Collaborate with engineering teams to improve test coverage."],
            ["Identify opportunities to reduce repetitive testing through automation.", "Support release readiness with documented test results.", "Improve traceability between requirements and test coverage."]),
        ("Full Stack Developer",
            ["full stack developer", "full-stack developer", "full stack", "react", "asp.net core"],
            ["C#", ".NET 8", "ASP.NET Core", "React", "TypeScript", "Azure", "Git", "SQL Server"],
            ["Develop and maintain application features across user interfaces and services.", "Design and implement REST APIs based on agreed requirements.", "Participate in code reviews, testing, and delivery improvements."],
            ["Deliver maintainable features aligned with product requirements.", "Contribute to application reliability through testing and code review.", "Improve maintainability through consistent engineering practices."]),
        ("Software Developer",
            ["software developer", "software engineer", "programmer", "c#", ".net", "java developer"],
            ["C#", ".NET 8", "ASP.NET Core", "React", "TypeScript", "Azure", "Git", "SQL Server"],
            ["Develop enterprise web applications.", "Design REST APIs.", "Participate in Agile development.", "Conduct code reviews.", "Implement CI/CD pipelines.", "Improve application performance."],
            ["Deliver business-critical features.", "Improve system performance.", "Enhance code quality and deployment efficiency."]),
        ("DevOps Engineer",
            ["devops", "ci/cd", "infrastructure as code", "kubernetes", "terraform"],
            ["Azure", "Azure DevOps", "CI/CD", "Docker", "Terraform", "Git", "Monitoring"],
            ["Manage CI/CD pipelines.", "Automate deployments.", "Implement Infrastructure as Code.", "Manage cloud environments.", "Support monitoring and reliability."],
            ["Improve deployment repeatability.", "Strengthen service reliability through monitoring.", "Reduce manual effort through delivery automation."]),
        ("Business Analyst",
            ["business analyst", "business analysis", "requirements analyst", "requirements gathering"],
            ["Requirements Analysis", "Stakeholder Management", "Process Mapping", "SQL", "Agile"],
            ["Elicit and document business and functional requirements.", "Facilitate communication between stakeholders and delivery teams.", "Support validation of solutions against agreed requirements."],
            ["Improve requirement clarity through structured stakeholder collaboration.", "Support delivery teams with traceable acceptance criteria.", "Help validate delivered changes against agreed business needs."]),
        ("QA Engineer",
            ["qa engineer", "quality assurance", "quality engineer", "software tester"],
            ["Test Planning", "Functional Testing", "Regression Testing", "API Testing", "SQL", "Jira", "Git"],
            ["Plan and execute functional and regression testing.", "Document defects with clear reproduction steps.", "Collaborate with delivery teams to validate fixes and release readiness."],
            ["Improve defect visibility with clear test evidence.", "Support quality-focused releases through systematic validation.", "Strengthen collaboration between testing and delivery teams."]),
        ("Data Engineer",
            ["data engineer", "data engineering", "etl", "data pipeline", "spark"],
            ["SQL", "Python", "ETL", "Data Pipelines", "Azure", "Spark"],
            ["Build and maintain data pipelines based on documented requirements.", "Validate data quality and investigate pipeline issues.", "Collaborate with analytics and platform teams."],
            ["Improve data reliability through validation and monitoring.", "Deliver reusable data workflows aligned with documented needs.", "Support consistent data delivery through documented pipeline practices."]),
        ("Cloud Engineer",
            ["cloud engineer", "cloud infrastructure", "azure cloud", "aws cloud"],
            ["Azure", "AWS", "Cloud Infrastructure", "Monitoring", "Networking", "Infrastructure as Code"],
            ["Configure and support cloud resources according to approved designs.", "Monitor services and investigate infrastructure issues.", "Collaborate with application teams on secure, reliable deployments."],
            ["Improve operational visibility through monitoring and documentation.", "Support reliable cloud services with repeatable practices.", "Contribute to secure and maintainable cloud operations."]),
    ];

    public static CvAiProfile Build(string extractedText)
    {
        var text = extractedText ?? string.Empty;
        var sections = ExtractSections(text);
        var roleProfile = RoleProfiles
            .Select(candidate => (Profile: candidate, Score: candidate.Terms.Sum(term =>
                text.Contains(term, StringComparison.OrdinalIgnoreCase) ? term.Contains(' ') ? 3 : 1 : 0)))
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .Select(item => item.Profile)
            .FirstOrDefault();
        var role = roleProfile.Role ?? "Role to confirm";
        var detectedSkills = Bound((sections.GetValueOrDefault("skills") ?? [])
            .SelectMany(item => item.Split([',', ';', '|', '•', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Concat(roleProfile.Skills?.Where(skill => text.Contains(skill, StringComparison.OrdinalIgnoreCase)) ?? [])
            .Where(item => item.Length is > 1 and <= 100)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray(), 100, 100);
        var detectedSkillSet = detectedSkills.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skills = detectedSkills
            .Concat((roleProfile.Skills ?? [])
                .Where(skill => !detectedSkillSet.Contains(skill))
                .Select(skill => $"Suggested (verify): {skill}"))
            .Take(100)
            .ToArray();
        if (skills.Length == 0)
        {
            skills = ["Add relevant technical skills before sharing."];
        }
        var name = FindName(text);
        var email = EmailRegex().Match(text).Value;
        var phone = PhoneRegex().Match(text).Value.Trim();
        var location = LocationRegex().Match(text).Groups[1].Value.Trim();
        var summary = string.Join(" ", sections.GetValueOrDefault("summary") ?? []).Trim();
        if (string.IsNullOrWhiteSpace(summary))
        {
            summary = skills.Length > 0
                ? $"Suggested summary — {role} profile with skills relevant to {role}. Add verified experience, industries, and outcomes before sharing."
                : "Draft summary — add a brief, verified overview of your experience and strengths before sharing.";
        }
        summary = summary[..Math.Min(summary.Length, 5000)];
        var responsibilities = roleProfile.Responsibilities is null
            ? ["Add confirmed responsibilities for your most relevant role."]
            : roleProfile.Responsibilities;
        var achievements = roleProfile.Achievements is null
            ? ["Add measurable achievements only when supported by your work history."]
            : roleProfile.Achievements;
        var experience = Bound(sections.GetValueOrDefault("experience") ?? [], 5000, 20);
        if (experience.Count == 0)
        {
            experience =
            [
                $"Suggested responsibilities — please verify before use:\n{string.Join("\n", responsibilities.Select(item => $"• {item}"))}"
            ];
        }

        var achievementItems = Bound(sections.GetValueOrDefault("achievements") ?? [], 500, 30);
        if (achievementItems.Count == 0)
        {
            achievementItems =
            [
                $"Suggested achievements — replace with verified outcomes:\n{string.Join("\n", achievements.Select(item => $"• {item}"))}"
            ];
        }

        return new CvAiProfile
        {
            FullName = name,
            TargetRole = role,
            Email = email,
            Phone = phone,
            Location = location.Length <= 300 ? location : location[..300],
            Summary = summary,
            Skills = skills,
            Experience = experience,
            Achievements = achievementItems,
            Projects = sections.GetValueOrDefault("projects") is { Count: > 0 } projects
                ? Bound(projects, 5000, 20)
                : ["Suggested Project - Please Review: Enterprise Digital Transformation Platform\n• Requirements gathering\n• Design\n• Development\n• Testing\n• Deployment\n• Production support"],
            Education = Bound(sections.GetValueOrDefault("education") ?? [], 3000, 20) is { Count: > 0 } education
                ? education
                : ["Add verified education details before sharing."],
            Certifications = Bound(sections.GetValueOrDefault("certifications") ?? [], 160, 20) is { Count: > 0 } certifications
                ? certifications
                : ["Add verified certifications before sharing."],
            Languages = Bound(sections.GetValueOrDefault("languages") ?? [], 100, 20),
            AdditionalInformation = Bound(sections.GetValueOrDefault("additionalInformation") ?? [], 1000, 20),
        };
    }

    private static Dictionary<string, IReadOnlyList<string>> ExtractSections(string text)
    {
        var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        string? activeSection = null;
        foreach (var rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IsFormattingNoise(rawLine))
            {
                continue;
            }

            var separator = rawLine.IndexOf(':');
            var heading = NormalizeHeading(separator >= 0 ? rawLine[..separator] : rawLine);
            var remainder = separator >= 0 ? rawLine[(separator + 1)..].Trim() : string.Empty;
            var section = GetSection(heading);
            if (section is not null)
            {
                activeSection = section;
                sections.TryAdd(section, []);
                if (!string.IsNullOrWhiteSpace(remainder))
                {
                    sections[section].Add(remainder);
                }

                continue;
            }

            if (activeSection is not null)
            {
                sections[activeSection].Add(rawLine.TrimStart(' ', '•', '-', '*'));
            }
        }

        return sections.ToDictionary(
            item => item.Key,
            item => (IReadOnlyList<string>)item.Value.Where(value => !string.IsNullOrWhiteSpace(value)).Take(30).ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static string? GetSection(string heading)
    {
        var exactMatch = GetExactSection(heading);
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        foreach (var delimiter in new[] { " | ", " — ", " – ", " - " })
        {
            var separator = heading.IndexOf(delimiter, StringComparison.Ordinal);
            if (separator > 0 && GetExactSection(heading[..separator].Trim()) is { } section)
            {
                return section;
            }
        }

        return null;
    }

    private static string? GetExactSection(string heading) =>
        heading switch
        {
            "summary" or "professional summary" or "profile" or "objective" or "career objective" or "career summary" => "summary",
            "skills" or "technical skills" or "core skills" or "key skills" or "areas of expertise" => "skills",
            "experience" or "work experience" or "professional experience" or "employment history" or "career history" or "work history" or "employment" => "experience",
            "education" or "educational background" or "academic background" or "academic qualifications" or "educational qualifications" or "education qualifications" => "education",
            "certifications" or "professional certifications" or "certificates" or "licenses" or "licenses and certifications" => "certifications",
            "projects" or "selected projects" or "key projects" or "project experience" => "projects",
            "languages" => "languages",
            "achievements" or "key achievements" or "accomplishments" or "awards" or "honors and awards" => "achievements",
            "additional information" or "additional details" or "other information" => "additionalInformation",
            _ => null,
        };

    private static string NormalizeHeading(string heading) =>
        Regex.Replace(
                heading.Trim().Trim(' ', '#', '*', '-', '•', ':')
                    .ToLowerInvariant(),
                @"^\d+[\s.)-]+",
                string.Empty)
            .Trim();

    private static bool IsFormattingNoise(string line) =>
        line is "}" or "{" or "</style>" or "<style>"
        || Regex.IsMatch(line, @"^(?:[a-z][\w-]*|[.#][\w-]+|[^{}]+)\s*\{\s*$", RegexOptions.IgnoreCase)
        || Regex.IsMatch(
            line,
            @"^(?:text-decoration|color|border(?:-[\w-]+)?|background(?:-[\w-]+)?|font(?:-[\w-]+)?|padding(?:-[\w-]+)?|margin(?:-[\w-]+)?|display|width|height|align-items)\s*:",
            RegexOptions.IgnoreCase);

    private static IReadOnlyList<string> Bound(IEnumerable<string> items, int maxLength, int maxCount) =>
        items.Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim()[..Math.Min(item.Trim().Length, maxLength)])
            .Take(maxCount)
            .ToArray();

    private static string FindName(string text)
    {
        var explicitName = NameLabelRegex().Match(text);
        if (explicitName.Success)
        {
            return CleanName(explicitName.Groups[1].Value);
        }

        var firstLines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !IsFormattingNoise(line))
            .Take(8);
        foreach (var line in firstLines)
        {
            var candidate = CleanName(line);
            if (line.Length is < 4 or > 70
                || line.Contains('@')
                || line.Any(char.IsDigit)
                || line.Contains(':')
                || candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length is < 2 or > 5
                || Regex.IsMatch(candidate, @"\b(resume|curriculum vitae|professional summary|experience|education|skills|contact|personal information|technical skills)\b", RegexOptions.IgnoreCase)
                || RoleProfiles.Any(profile => profile.Terms.Any(term => candidate.Equals(term, StringComparison.OrdinalIgnoreCase))))
            {
                continue;
            }

            return candidate;
        }

        return "Your Name";
    }

    private static string CleanName(string value)
    {
        var name = value.Split(['|', '•', '·'], 2, StringSplitOptions.TrimEntries)[0];
        name = EmailRegex().Replace(name, string.Empty);
        name = PhoneRegex().Replace(name, string.Empty);
        name = Regex.Replace(name, @"\s{2,}", " ").Trim(' ', ',', ';', '-', '–');
        return name.Length <= 200 ? name : name[..200];
    }

    [GeneratedRegex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?:\+\d[\d ().-]{5,}\d|\(\d{3}\)\s*\d{3}[- .]\d{4}|\b\d{10,15}\b)", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"(?:full\s+name|candidate\s+name|name)\s*[:\-]\s*([^\r\n]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NameLabelRegex();

    [GeneratedRegex(@"(?:location|address|based\s+in)\s*[:\-]\s*([^\r\n]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LocationRegex();
}
