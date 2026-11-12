import { useEffect, useRef, useState } from "react";
import { motion, useReducedMotion } from "framer-motion";
import {
  Badge,
  Button,
  Card,
  Input,
  Textarea,
  Text,
  makeStyles,
} from "@fluentui/react-components";
import { ArrowDownload20Regular, ArrowUp20Regular, DocumentPdf20Regular, Edit20Regular, Save20Regular } from "@fluentui/react-icons";
import type { CSSProperties, ReactNode } from "react";
import type { CvDesignConfiguration, CvProfile } from "../types";
import { calculateCompletion, hasProfileContent, hasProfileItems } from "../utils/resumeParser";

const useStyles = makeStyles({
  card: { padding: "20px", minHeight: "520px" },
  preview: { minHeight: "430px", padding: "22px", backgroundColor: "#fff" },
  name: { display: "block", marginBottom: "4px" },
  section: { marginTop: "22px" },
  sectionTitle: {
    display: "block",
    marginBottom: "8px",
    paddingBottom: "5px",
    borderBottom: "1px solid #d6edf8",
    textTransform: "uppercase",
    letterSpacing: "0.08em",
  },
  actions: { display: "flex", flexWrap: "wrap", gap: "8px", marginTop: "14px" },
});

interface CvPreviewProps {
  profile: CvProfile;
  templateId: string;
  designConfiguration: CvDesignConfiguration;
  designMode?: boolean;
  onExportDocx: () => void;
  onExportPdf: () => void;
  onSave: (profile: CvProfile) => Promise<boolean>;
  saving: boolean;
  disabled: boolean;
}

export default function CvPreview({
  profile,
  templateId,
  designConfiguration,
  designMode = false,
  onExportDocx,
  onExportPdf,
  onSave,
  saving,
  disabled,
}: CvPreviewProps) {
  const styles = useStyles();
  const prefersReducedMotion = useReducedMotion();
  const [editing, setEditing] = useState(false);
  const [editedProfile, setEditedProfile] = useState(profile);
  const previewScrollRef = useRef<HTMLDivElement>(null);
  const currentProfile = editing ? editedProfile : profile;
  const contact = [currentProfile.email, currentProfile.phone, currentProfile.location, currentProfile.linkedIn].filter(Boolean).join("  |  ");
  const completion = calculateCompletion(currentProfile);
  const dirty = JSON.stringify(editedProfile) !== JSON.stringify(profile);
  const documentStyle: CSSProperties & Record<`--cv-${string}`, string> = {
    "--cv-primary": designConfiguration.primaryColor,
    "--cv-secondary": designConfiguration.secondaryColor,
    "--cv-light": designConfiguration.lightColor,
    "--cv-font-family": designConfiguration.fontFamily,
    "--cv-font-size": `${designConfiguration.fontSize}px`,
    "--cv-section-spacing": `${designConfiguration.sectionSpacing}px`,
  };
  const sectionContent: Record<string, ReactNode> = {
    summary: editing
      ? <Textarea aria-label="Professional Summary" resize="vertical" value={editableText(currentProfile.summary)} onChange={(_, data) => setText("summary", data.value)} />
      : currentProfile.summary || "Please add this information.",
    skills: editing ? (
      <EditableList value={currentProfile.skills} label="Technical Skills" onChange={(value) => setItems("skills", value)} />
    ) : hasProfileItems(currentProfile.skills) ? (
      <div className="skill-list">
        <Text className="skill-note" size={100}>Skills are grouped by topic for easier review. Please confirm each item.</Text>
        {groupSkills(currentProfile.skills).map((group) => (
          <section className="skill-category" key={group.label}>
            <div className="skill-category-heading">
              <Text className="skill-category-title" size={200} weight="semibold">{group.label}</Text>
              <Text className="skill-category-count" size={100}>
                {group.items.length} {group.items.length === 1 ? "item" : "items"}
              </Text>
            </div>
            <div className="skill-category-items">
              {group.items.map((skill, index) => <span className="skill-chip" key={`${skill}-${index}`}>{skill}</span>)}
            </div>
          </section>
        ))}
      </div>
    ) : "Please add this information.",
    experience: editing
      ? <EditableList value={currentProfile.experience} label="Professional Experience" onChange={(value) => setItems("experience", value)} />
      : formatEntries(currentProfile.experience, "Experience"),
    achievements: editing
      ? <EditableList value={currentProfile.achievements} label="Key Achievements" onChange={(value) => setItems("achievements", value)} />
      : formatEntries(currentProfile.achievements, "Achievements"),
    projects: editing
      ? <EditableList value={currentProfile.projects} label="Projects" onChange={(value) => setItems("projects", value)} />
      : formatEntries(currentProfile.projects, "Projects"),
    certifications: editing
      ? <EditableList value={currentProfile.certifications} label="Certifications" onChange={(value) => setItems("certifications", value)} />
      : formatEntries(currentProfile.certifications, "Certifications"),
    education: editing
      ? <EditableList value={currentProfile.education} label="Education" onChange={(value) => setItems("education", value)} />
      : formatEntries(currentProfile.education, "Education"),
    additional: editing ? (
      <>
        <EditableList value={currentProfile.languages.filter((item) => !isPlaceholder(item))} label="Languages" onChange={(value) => setItems("languages", value)} />
        <EditableList value={currentProfile.additionalInformation} label="Additional Information" onChange={(value) => setItems("additionalInformation", value)} />
      </>
    ) : formatEntries([...currentProfile.languages, ...currentProfile.additionalInformation], "Languages"),
  };

  useEffect(() => setEditedProfile(profile), [profile]);

  function setText(field: "fullName" | "targetRole" | "email" | "phone" | "location" | "linkedIn" | "summary", value: string) {
    setEditedProfile((current) => ({ ...current, [field]: value }));
  }

  function setItems(field: "skills" | "experience" | "achievements" | "projects" | "certifications" | "education" | "languages" | "additionalInformation", value: string) {
    setEditedProfile((current) => ({
      ...current,
      [field]: value.split(/\r?\n/).map((item) => item.trim()).filter(Boolean),
    }));
  }

  async function saveEdits() {
    if (await onSave(editedProfile)) setEditing(false);
  }

  return (
    <Card className={styles.card}>
      {!designMode && (
        <div className="panel-heading preview-heading">
          <div>
            <Text weight="semibold" size={400}>CV Preview</Text>
            <Text className="muted-text" size={200}>NTT DATA Professional Template</Text>
          </div>
          <Badge appearance="tint" color="success">{dirty ? "Edits pending" : "Current"}</Badge>
        </div>
      )}

      {!designMode && <div className="profile-completion">
        <div className="profile-completion-label">
          <Text size={200} weight="semibold">Profile Completeness</Text>
          <Text size={200} weight="semibold">{completion}% Complete</Text>
        </div>
        <div
          className="profile-completion-track"
          role="progressbar"
          aria-label="Profile completion"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={completion}
        >
          <motion.div
            className="profile-completion-value"
            initial={{ width: 0 }}
            animate={{ width: `${completion}%` }}
            transition={{ duration: 0.3, ease: "easeOut" }}
          />
        </div>
        <div className="completion-requirements">
          <span className={hasProfileContent(currentProfile.fullName) && (hasProfileContent(currentProfile.email) || hasProfileContent(currentProfile.phone)) ? "complete" : ""}>Personal information · 10%</span>
          <span className={hasText(currentProfile.summary) ? "complete" : ""}>Summary · 10%</span>
          <span className={hasItems(currentProfile.skills) ? "complete" : ""}>Skills · 25%</span>
          <span className={hasItems(currentProfile.experience) ? "complete" : ""}>Experience · 25%</span>
          <span className={hasItems(currentProfile.achievements) ? "complete" : ""}>Achievements · 10%</span>
          <span className={hasItems(currentProfile.projects) ? "complete" : ""}>Projects · 10%</span>
          <span className={hasItems(currentProfile.certifications) ? "complete" : ""}>Certifications · Shared 10%</span>
          <span className={hasItems(currentProfile.education) ? "complete" : ""}>Education · Shared 10%</span>
          <span className={hasItems([...currentProfile.languages, ...currentProfile.additionalInformation]) ? "complete" : ""}>Additional information · Shared 10%</span>
        </div>
        <Text className="completion-weight-note" size={100}>
          Certifications, Education, and Additional Information share 10%; each missing section reduces the score by 5 points.
        </Text>
      </div>}

      {!designMode && completion < 100 && (
        <div className="cv-missing-notice" role="status">
          <Text size={200}>Some information may require verification. Please review all sections before exporting.</Text>
          {!editing && (
            <Button appearance="subtle" size="small" icon={<Edit20Regular />} onClick={() => setEditing(true)}>
              Edit missing sections
            </Button>
          )}
        </div>
      )}

      <motion.section
        className="cv-window"
        style={documentStyle}
        initial={prefersReducedMotion ? false : { opacity: 0, y: 18, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={prefersReducedMotion ? { duration: 0 } : { duration: 0.48, ease: [0.22, 1, 0.36, 1] }}
        aria-label="Animated CV document preview"
      >
        <div className="cv-window-toolbar">
          <div className="cv-window-title">
            <span className="cv-window-controls" aria-hidden="true">
              <i />
              <i />
              <i />
            </span>
            <span className="cv-window-label">LIVE PREVIEW</span>
          </div>
          <div className="cv-window-meta">
            <span>NTT DATA Professional CV</span>
            <Button
              appearance="subtle"
              size="small"
              icon={<ArrowUp20Regular />}
              aria-label="Scroll CV preview to top"
              onClick={() => previewScrollRef.current?.scrollTo({ top: 0, behavior: "smooth" })}
            >
              Top
            </Button>
          </div>
        </div>
        <div
          ref={previewScrollRef}
          className="cv-window-viewport"
          role="region"
          aria-label="Scrollable CV document"
          tabIndex={0}
        >
        <motion.article
          className={`cv-paper cv-paper-${templateId} cv-paper-density-${designConfiguration.layout} ${styles.preview} ntt-cv-paper`}
          initial={prefersReducedMotion ? false : { opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={prefersReducedMotion ? { duration: 0 } : { duration: 0.35, delay: 0.08 }}
        >
        <header className="cv-header ntt-cv-header">
          <div className="cv-brand-row">
            {designConfiguration.showLogo && <img src="/assets/ntt-logo.png" alt="NTT DATA" />}
            <span>PROFESSIONAL CV</span>
          </div>
          <Text className="ntt-cv-section-title" size={200} weight="semibold">PERSONAL INFORMATION</Text>
          {editing ? (
            <div className="cv-edit-grid cv-edit-personal">
              <Input aria-label="Full name" placeholder="Full name" value={currentProfile.fullName} onChange={(_, data) => setText("fullName", data.value)} />
              <Input aria-label="Target role" placeholder="Target role" value={currentProfile.targetRole} onChange={(_, data) => setText("targetRole", data.value)} />
              <Input aria-label="Email" placeholder="Email" value={currentProfile.email} onChange={(_, data) => setText("email", data.value)} />
              <Input aria-label="Phone" placeholder="Phone" value={currentProfile.phone} onChange={(_, data) => setText("phone", data.value)} />
              <Input aria-label="Location" placeholder="Location" value={currentProfile.location} onChange={(_, data) => setText("location", data.value)} />
              <Input aria-label="LinkedIn" placeholder="LinkedIn URL" value={currentProfile.linkedIn ?? ""} onChange={(_, data) => setText("linkedIn", data.value)} />
            </div>
          ) : (
            <>
              <Text className={styles.name} size={600} weight="bold">{currentProfile.fullName || "Name"}</Text>
              <Text className="cv-target-role" size={300} weight="semibold">{currentProfile.targetRole || "Target Role"}</Text>
              {contact && <Text className="cv-contact" size={200}>{contact}</Text>}
            </>
          )}
        </header>

        {[...designConfiguration.sections]
          .filter((section) => section.enabled)
          .sort((left, right) => left.order - right.order)
          .map((section) => (
            <CvSection key={section.id} title={section.title} className={styles.section} titleClassName={styles.sectionTitle}>
              {sectionContent[section.id] ?? ""}
            </CvSection>
          ))}
        {designConfiguration.customSections.map((section) => (
          <CvSection key={section.id} title={section.title} className={styles.section} titleClassName={styles.sectionTitle}>
            {section.content || "Please add this information."}
          </CvSection>
        ))}
        {designConfiguration.showFooter && <footer className="cv-template-footer">
          NTT DATA Professional CV Builder
        </footer>}
        </motion.article>
        </div>
      </motion.section>

      {!designMode && <div className={styles.actions}>
        {editing ? (
          <>
            <Button appearance="primary" icon={<Save20Regular />} disabled={disabled || saving || !dirty || !currentProfile.fullName.trim()} onClick={() => void saveEdits()}>
              {saving ? "Saving changes…" : "Save changes"}
            </Button>
            <Button appearance="subtle" disabled={saving} onClick={() => {
              setEditedProfile(profile);
              setEditing(false);
            }}>
              Cancel
            </Button>
          </>
        ) : (
          <Button appearance="secondary" icon={<Edit20Regular />} disabled={disabled} onClick={() => setEditing(true)}>
            Edit CV
          </Button>
        )}
        <Button
          className="ntt-primary-button"
          appearance="primary"
          icon={<DocumentPdf20Regular />}
          disabled={disabled || editing || dirty || saving}
          onClick={onExportPdf}
        >
          Download PDF
        </Button>
        <Button
          className="ntt-secondary-button"
          appearance="secondary"
          icon={<ArrowDownload20Regular />}
          disabled={disabled || editing || dirty || saving}
          onClick={onExportDocx}
        >
          Download Word
        </Button>
      </div>}
    </Card>
  );
}

function formatEntries(entries: string[], section: string): string {
  if (hasProfileItems(entries)) {
    return entries.filter(hasProfileContent).join("\n\n");
  }
  const missingContent: Record<string, string> = {
    Certifications: "Please add this information.",
    Experience: "Please add this information.",
    Projects: "Please add this information.",
    Education: "Please add this information.",
    Languages: "Please add this information.",
    Achievements: "Please add this information.",
  };
  return missingContent[section] ?? "Please add this information.";
}

function CvSection({
  title,
  children,
  className,
  titleClassName,
}: {
  title: string;
  children: ReactNode;
  className: string;
  titleClassName: string;
}) {
  return (
    <section className={`cv-section ${className}`}>
      <Text className={`ntt-cv-section-title ${titleClassName}`} size={200} weight="semibold">{title}</Text>
      <div className="cv-section-content">{children}</div>
    </section>
  );
}

function EditableList({
  value,
  label,
  onChange,
}: {
  value: string[];
  label: string;
  onChange: (value: string) => void;
}) {
  const text = value.filter((item) => !isPlaceholder(item)).join("\n");
  return (
    <Textarea
      aria-label={label}
      placeholder={`Add ${label.toLowerCase()}, one item per line`}
      resize="vertical"
      rows={Math.max(3, Math.min(7, text.split("\n").length))}
      value={text}
      onChange={(_, data) => onChange(data.value)}
    />
  );
}

function editableText(value: string): string {
  return isPlaceholder(value) ? "" : value;
}

function isPlaceholder(value: string): boolean {
  return /^(not provided|add (confirmed|verified|relevant)|please add)/i.test(value.trim());
}

function hasText(value: string): boolean {
  return hasProfileContent(value);
}

function hasItems(items: string[]): boolean {
  return hasProfileItems(items);
}

function groupSkills(skills: string[]): Array<{ label: string; items: string[] }> {
  const groups = new Map<string, string[]>();

  for (const rawSkill of skills) {
    const suggested = rawSkill.startsWith("Suggested (verify): ");
    const value = suggested ? rawSkill.slice("Suggested (verify): ".length) : rawSkill;

    for (const clause of value.split(/[\n;|•]+/)) {
      const trimmed = clause.trim();
      if (!trimmed) continue;

      const colonIndex = trimmed.indexOf(":");
      const hasCategory = colonIndex > 0 && colonIndex < 52;
      const label = suggested
        ? "Suggested — please verify"
        : hasCategory ? trimmed.slice(0, colonIndex).trim() : "Core skills";
      const list = hasCategory ? trimmed.slice(colonIndex + 1) : trimmed;
      const items = list.split(/[,،]+/).map((item) => item.trim()).filter(Boolean);
      if (!items.length) continue;

      const existing = groups.get(label) ?? [];
      for (const item of items) {
        if (!existing.some((current) => current.toLocaleLowerCase() === item.toLocaleLowerCase())) {
          existing.push(item);
        }
      }
      groups.set(label, existing);
    }
  }

  return Array.from(groups, ([label, items]) => ({ label, items }));
}
