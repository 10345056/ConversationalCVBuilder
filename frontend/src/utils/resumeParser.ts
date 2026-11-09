import type { CvProfile, ResumeAnalysisItem } from "../types";

const sectionPatterns: Record<string, RegExp> = {
  summary: /^(?:(?:professional\s+)?summary|profile|(?:career\s+)?objective|about(?:\s+me)?)$/i,
  skills: /^(?:(?:technical|core|key)\s+)?skills?$/i,
  experience: /^(?:(?:professional|work)\s+)?(?:experience|employment|work\s+history|career\s+history|employment\s+history)$/i,
  education: /^(?:education|educational\s+qualification|educational\s+qualifications|education\s+qualifications|academic\s+background|academic\s+qualifications)$/i,
  certifications: /^(?:(?:trainings?\s*(?:&|and)\s*)?certifications?|professional\s+certifications|licenses?(?:\s+and\s+certifications)?)$/i,
  projects: /^(?:(?:(?:selected|key)\s+)?projects?|project\s+experience)$/i,
  languages: /^(?:spoken\s+)?languages?$/i,
  achievements: /^(?:key\s+)?(?:achievements?|accomplishments?|awards?|honors?\s+and\s+awards?)$/i,
  additionalInformation: /^(?:additional\s+(?:information|details)|other\s+information)$/i,
};

export function parseResumeText(text: string): { profile: CvProfile } {
  const lines = separateInlineSections(text).split(/\r?\n/).map((line) => line.replace(/\f/g, "").trim()).filter(Boolean);
  const usableLines = lines.filter((line) => !/^[-_=*•\s]+$/.test(line) && !isFormattingNoise(line));
  const sections: Record<string, string[]> = {};
  let activeSection = "";
  const unsectioned: string[] = [];

  for (const rawLine of usableLines) {
    const heading = matchSectionHeading(rawLine);
    const section = heading?.section;
    if (section) {
      activeSection = section;
      sections[section] ??= [];
      if (heading.remainder) sections[section].push(heading.remainder);
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
  const additionalInformation = boundItems(splitItems((sections.additionalInformation ?? []).join("\n")), 1000, 20);

  if (!fullName) {
    return {
      profile: {
        fullName: "",
        targetRole: "",
        email: "",
        phone: "",
        location: "",
        linkedIn: "",
        summary: "",
        skills: [],
        experience: [],
        education: [],
        certifications: [],
        projects: [],
        languages: [],
        achievements: [],
        additionalInformation: [],
      },
    };
  }

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
      additionalInformation,
    },
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
  const weightedSections = [
    { complete: hasProfileContent(profile.fullName) && (hasProfileContent(profile.email) || hasProfileContent(profile.phone)), weight: 10 },
    { complete: hasProfileContent(profile.summary), weight: 10 },
    { complete: hasProfileItems(profile.skills), weight: 25 },
    { complete: hasProfileItems(profile.experience), weight: 25 },
    { complete: hasProfileItems(profile.achievements), weight: 10 },
    { complete: hasProfileItems(profile.projects), weight: 10 },
  ];
  const supplemental = [
    hasProfileItems(profile.certifications),
    hasProfileItems(profile.education),
    hasProfileItems([...profile.languages, ...profile.additionalInformation]),
  ];
  const missingSupplementalPenalty = supplemental.filter((complete) => !complete).length * (5 / 3);
  const baseScore = weightedSections.reduce(
    (score, section) => score + (section.complete ? section.weight : 0),
    0,
  );
  const supplementalScore = supplemental.filter(Boolean).length * (10 / 3);
  return Math.round(Math.max(0, baseScore + supplementalScore - missingSupplementalPenalty));
}

export function hasProfileContent(value: string): boolean {
  const normalized = value.trim().toLowerCase();
  return normalized.length > 0
    && !/(not provided|add (confirmed|verified|relevant)|add languages or other relevant information|please add|suggested|draft summary|please verify|cv builder user|your name|target role)/i.test(normalized);
}

export function hasProfileItems(items: string[]): boolean {
  return items.some(hasProfileContent);
}

function headingsForLine(line: string): boolean {
  return Boolean(matchSectionHeading(line));
}

function matchSectionHeading(line: string): { section: string; remainder: string } | undefined {
  const headingMatch = line.match(/^([^:]{2,60}):\s*(.*)$/);
  const heading = normalizeHeading((headingMatch?.[1] ?? line).replace(/[\s:|–-]+$/, ""));
  const exactSection = findSection(heading);
  if (exactSection) return { section: exactSection, remainder: headingMatch?.[2]?.trim() ?? "" };

  if (!headingMatch) {
    const suffix = heading.match(/^(.+?)\s+(?:\||—|–|-)\s+(.+)$/);
    const section = suffix ? findSection(suffix[1]) : undefined;
    if (section && suffix) return { section, remainder: suffix[2].trim() };
  }
  return undefined;
}

function findSection(heading: string): string | undefined {
  return Object.entries(sectionPatterns).find(([, pattern]) => pattern.test(heading))?.[0];
}

function normalizeHeading(value: string): string {
  return value
    .replace(/^\s*\d+[\s.)-]+/, "")
    .replace(/^[#*•\s]+|[#*•\s]+$/g, "")
    .trim();
}

function isFormattingNoise(line: string): boolean {
  return /^(?:[a-z][\w-]*|[.#][\w-]+|[^{}]+)\s*\{\s*$/i.test(line)
    || /^(?:text-decoration|color|border(?:-[\w-]+)?|background(?:-[\w-]+)?|font(?:-[\w-]+)?|padding(?:-[\w-]+)?|margin(?:-[\w-]+)?|display|width|height|align-items)\s*:/i.test(line)
    || /^(?:[{}]|<\/?style\b)/i.test(line);
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
    "career history",
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
    "academic qualifications",
    "key projects",
    "education",
    "languages",
    "achievements?",
    "awards?",
    "additional information",
  ].join("|");
  return text.replace(
    new RegExp(`(?:(?<=[.!?])\\s+|\\s{2,})(?=(?:${headings})\\s*:)`, "gi"),
    "\n",
  );
}
