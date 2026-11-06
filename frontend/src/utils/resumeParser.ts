import type { CvProfile, ResumeAnalysisItem } from "../types";

const sectionPatterns: Record<string, RegExp> = {
  summary: /^(?:(?:professional\s+)?summary|profile|(?:career\s+)?objective|about(?:\s+me)?)$/i,
  skills: /^(?:(?:technical|core|key)\s+)?skills?$/i,
  experience: /^(?:(?:professional|work)\s+)?(?:experience|employment|work\s+history)$/i,
  education: /^(?:education|educational\s+qualification|academic\s+background)$/i,
  certifications: /^(?:(?:trainings?\s*(?:&|and)\s*)?certifications?|licenses?)$/i,
  projects: /^(?:selected\s+)?projects?$/i,
  languages: /^(?:spoken\s+)?languages?$/i,
  achievements: /^(?:key\s+)?(?:achievements?|accomplishments?|awards?)$/i,
};

const sampleProfile: CvProfile = {
  fullName: "Jordan Davis",
  targetRole: "Senior Product Designer",
  email: "jordan.davis@example.com",
  phone: "+1 415 555 0138",
  location: "San Francisco, CA",
  summary:
    "Customer-focused product designer with 8+ years of experience turning complex challenges into clear, accessible digital experiences. Collaborative leader who connects user research, product strategy, and measurable outcomes.",
  skills: ["Product strategy", "UX research", "Figma", "Design systems", "Accessibility", "Leadership"],
  experience: [
    "Senior Product Designer | Northstar Digital | 2021–Present\nLed discovery and design for customer platforms used by 200,000+ people, improving task completion by 32%.",
    "Product Designer | Brightworks | 2017–2021\nBuilt a scalable design system and partnered with engineering to launch responsive products across web and mobile.",
  ],
  education: ["B.A. Interaction Design | California College of the Arts | 2017"],
  certifications: ["Certified Professional in Accessibility Core Competencies (CPACC) | IAAP"],
  projects: ["Customer onboarding redesign | Reduced time to first value by 28% through research-led improvements."],
  languages: ["English — Native", "Spanish — Professional"],
  achievements: ["2023 Northstar Customer Impact Award", "Presented design system practices at ProductCon 2022"],
};

export function parseResumeText(text: string): { profile: CvProfile; usedSampleData: boolean } {
  const lines = separateInlineSections(text).split(/\r?\n/).map((line) => line.replace(/\f/g, "").trim()).filter(Boolean);
  const usableLines = lines.filter((line) => !/^[-_=*•\s]+$/.test(line));
  const sections: Record<string, string[]> = {};
  let activeSection = "";
  const unsectioned: string[] = [];

  for (const rawLine of usableLines) {
    const headingMatch = rawLine.match(/^([^:]{2,60}):\s*(.*)$/);
    const heading = (headingMatch?.[1] ?? rawLine).replace(/[\s:|–-]+$/, "").trim();
    const section = Object.entries(sectionPatterns).find(([, pattern]) => pattern.test(heading))?.[0];
    if (section) {
      activeSection = section;
      sections[section] ??= [];
      if (headingMatch?.[2]) sections[section].push(headingMatch[2].trim());
      continue;
    }

    if (activeSection && headingsForLine(rawLine)) activeSection = "";
    if (activeSection) sections[activeSection].push(rawLine);
    else unsectioned.push(rawLine);
  }

  const fullText = usableLines.join("\n");
  const email = fullText.match(/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b/i)?.[0] ?? "";
  const phone = usableLines
    .map((line) => line.match(/(?:\+\d[\d ().-]{5,}\d|\(\d{3}\)\s*\d{3}[- .]\d{4}|\b\d{10,15}\b)/)?.[0]?.trim())
    .find((candidate) => {
      const digits = candidate?.replace(/\D/g, "").length ?? 0;
      return digits >= 7 && digits <= 20;
    }) ?? "";
  const nameFromLabel = fullText.match(/(?:full\s+name|candidate\s+name|name)\s*[:\-]\s*([^\n]+)/i)?.[1]?.trim();
  const nameFromHeader = fullText.match(
    /^\s*([A-Z][A-Z.'’-]*(?:\s+[A-Z][A-Z.'’-]*){1,4})\s+(?=(?:e-?mail|mobile|phone|contact)\b)/i,
  )?.[1]?.trim();
  const nameFromLines = unsectioned.find((line) =>
    !/@/.test(line) &&
    !/\d{3,}/.test(line) &&
    !/^(?:resume|curriculum vitae|cv)$/i.test(line) &&
    line.split(/\s+/).length >= 2 &&
    line.length < 65 &&
    !/[.!?]$/.test(line),
  );
  const headerParts = nameFromLines?.split(/\s+[|•·]\s+/) ?? [];
  const nameCandidate = nameFromLabel ?? nameFromHeader ?? headerParts[0] ?? "";
  const normalizedName = nameFromHeader && nameCandidate === nameFromHeader
    ? nameCandidate.toLowerCase().replace(/\b[a-z]/g, (letter) => letter.toUpperCase())
    : nameCandidate;
  const parsedName = cleanContact(normalizedName).split(/\s+/);
  const fullName = [parsedName[0]?.slice(0, 100), parsedName.slice(1).join(" ").slice(0, 100)]
    .filter(Boolean)
    .join(" ");
  const targetRole = cleanContact(
    fullText.match(/(?:target\s+role|professional\s+title|job\s+title|headline)\s*[:\-]\s*([^\n]+)/i)?.[1]
      ?? headerParts.slice(1).join(" | "),
  ).slice(0, 160);
  const location = cleanContact(fullText.match(/(?:location|address|based\s+in)\s*[:\-]\s*([^\n]+)/i)?.[1] ?? "").slice(0, 300);
  const summary = normalizeParagraph(sections.summary?.join(" ") ?? "").slice(0, 5000);
  const skillText = sections.skills?.join(", ") ?? "";
  const skills = boundItems(splitItems(skillText), 100, 100);
  const experience = boundItems(splitEntries(sections.experience ?? []), 5000, 20);
  const education = boundItems(splitEntries(sections.education ?? []), 3000, 20);
  const certifications = boundItems(splitItems((sections.certifications ?? []).join("\n")), 160, 20);
  const projects = boundItems(splitEntries(sections.projects ?? []), 5000, 20);
  const languages = boundItems(splitItems((sections.languages ?? []).join("\n")), 100, 20);
  const achievements = boundItems(splitEntries(sections.achievements ?? []), 500, 30);

  const hasUsableContent = Boolean(fullName);
  if (!hasUsableContent) return { profile: sampleProfile, usedSampleData: true };

  const fallbackSummary = summary || unsectioned
    .filter((line) => line !== nameFromLines && line !== email && line !== phone && line.length > 70)
    .slice(0, 3)
    .join(" ");

  return {
    profile: {
      fullName,
      targetRole,
      email,
      phone,
      location,
      summary: normalizeParagraph(fallbackSummary).slice(0, 5000),
      skills,
      experience,
      education,
      certifications,
      projects,
      languages,
      achievements,
    },
    usedSampleData: false,
  };
}

export function analyzeProfile(profile: CvProfile): ResumeAnalysisItem[] {
  return [
    { label: "Name", found: Boolean(profile.fullName.trim()) },
    { label: "Skills", found: profile.skills.length > 0 },
    { label: "Experience", found: profile.experience.length > 0 },
    { label: "Certifications", found: profile.certifications.length > 0 },
    { label: "Languages", found: profile.languages.length > 0 },
  ];
}

export function calculateCompletion(profile: CvProfile): number {
  const completeSections = [
    Boolean(profile.fullName.trim() && (profile.email.trim() || profile.phone.trim())),
    Boolean(profile.summary.trim()),
    profile.skills.length > 0,
    profile.experience.length > 0,
    profile.education.length > 0,
    profile.certifications.length > 0,
  ].filter(Boolean).length;
  return Math.round((completeSections / 6) * 100);
}

function headingsForLine(line: string): boolean {
  const normalized = line.replace(/[\s:|–-]+$/, "").trim();
  return Object.values(sectionPatterns).some((pattern) => pattern.test(normalized));
}

function cleanContact(value: string): string {
  return value
    .replace(/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b/gi, "")
    .replace(/(?:\+\d[\d ().-]{5,}\d|\(\d{3}\)\s*\d{3}[- .]\d{4}|\b\d{10,15}\b)/g, "")
    .replace(/\s*[|,;]\s*/g, " ")
    .replace(/\s{2,}/g, " ")
    .trim();
}

function splitItems(value: string): string[] {
  return value
    .split(/\n|[,;|•·]+/)
    .map((item) => item.replace(/^[\s•·\-–]+|[\s•·\-–]+$/g, "").trim())
    .filter((item) => item.length > 1)
    .filter((item, index, items) => items.findIndex((other) => other.toLowerCase() === item.toLowerCase()) === index);
}

function splitEntries(lines: string[]): string[] {
  const entries = lines.join("\n")
    .split(/\n{2,}|(?<=\.)\s+(?=[A-Z][A-Za-z ]{2,45}\s+\|\s+)/)
    .map((entry) => entry.replace(/^[\s•·\-–]+|[\s•·\-–]+$/g, "").trim())
    .filter((entry) => entry.length > 2);
  return entries.filter((entry, index) => entries.indexOf(entry) === index);
}

function boundItems(items: string[], maxLength: number, maxCount: number): string[] {
  return items.slice(0, maxCount).map((item) => item.slice(0, maxLength));
}

function normalizeParagraph(value: string): string {
  return value.replace(/\s+/g, " ").trim();
}

function separateInlineSections(text: string): string {
  const headings = [
    "career objective",
    "professional summary",
    "professional experience",
    "work experience",
    "experience",
    "trainings?\\s*(?:&|and)\\s*certifications",
    "certifications",
    "technical skills",
    "skills",
    "projects?",
    "educational qualification",
    "education",
    "languages",
    "achievements?",
    "awards?",
  ].join("|");
  return text.replace(
    new RegExp(`(?:(?<=[.!?])\\s+|\\s{2,})(?=(?:${headings})\\s*:)`, "gi"),
    "\n",
  );
}
