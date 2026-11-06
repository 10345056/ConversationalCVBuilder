import { motion } from "framer-motion";
import {
  Badge,
  Button,
  Card,
  Text,
  makeStyles,
} from "@fluentui/react-components";
import { ArrowDownload20Regular, DocumentPdf20Regular } from "@fluentui/react-icons";
import type { CvProfile } from "../types";
import { calculateCompletion } from "../utils/resumeParser";
import type { ReactNode } from "react";

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
  onExportDocx: () => void;
  onExportPdf: () => void;
  disabled: boolean;
}

export default function CvPreview({
  profile,
  templateId,
  onExportDocx,
  onExportPdf,
  disabled,
}: CvPreviewProps) {
  const styles = useStyles();
  const contact = [profile.email, profile.phone, profile.location, profile.linkedIn].filter(Boolean).join("  |  ");
  const completion = calculateCompletion(profile);

  return (
    <Card className={styles.card}>
      <div className="panel-heading preview-heading">
        <div>
          <Text weight="semibold" size={400}>CV Preview</Text>
          <Text className="muted-text" size={200}>NTT DATA Professional Template</Text>
        </div>
        <Badge appearance="tint" color="success">Live</Badge>
      </div>

      <div className="profile-completion">
        <div className="profile-completion-label">
          <Text size={200} weight="semibold">Profile Completion</Text>
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
          <span className={profile.fullName.trim() && (profile.email.trim() || profile.phone.trim()) ? "complete" : ""}>Personal details</span>
          <span className={profile.summary.trim() ? "complete" : ""}>Summary</span>
          <span className={profile.skills.length ? "complete" : ""}>Skills</span>
          <span className={profile.experience.length ? "complete" : ""}>Experience</span>
          <span className={profile.education.length ? "complete" : ""}>Education</span>
          <span className={profile.certifications.length ? "complete" : ""}>Certifications</span>
        </div>
      </div>

      <motion.article
        className={`cv-paper cv-paper-${templateId} ${styles.preview} ntt-cv-paper`}
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        <header className="cv-header ntt-cv-header">
          <div className="cv-brand-row">
            <img src="/assets/ntt-logo.png" alt="NTT DATA" />
            <span>PROFESSIONAL CV</span>
          </div>
          <Text className={styles.name} size={600} weight="bold">
            {profile.fullName || "Your name"}
          </Text>
          <Text className="cv-target-role" size={300} weight="semibold">
            {profile.targetRole || "Target Role"}
          </Text>
          {contact && <Text className="cv-contact" size={200}>{contact}</Text>}
        </header>

        <CvSection title="Professional Summary" className={styles.section} titleClassName={styles.sectionTitle}>
          {profile.summary || "Add a concise summary of your professional experience and strengths."}
        </CvSection>
        <CvSection title="Skills" className={styles.section} titleClassName={styles.sectionTitle}>
          {profile.skills.length ? (
            <div className="skill-list">
              {profile.skills.map((skill) => <Badge key={skill} appearance="outline">{skill}</Badge>)}
            </div>
          ) : "Add relevant skills to highlight your strengths."}
        </CvSection>
        <CvSection title="Certifications" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.certifications, "Certifications")}
        </CvSection>
        <CvSection title="Experience" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.experience, "Experience")}
        </CvSection>
        <CvSection title="Projects" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.projects, "Projects")}
        </CvSection>
        <CvSection title="Education" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.education, "Education")}
        </CvSection>
        <CvSection title="Languages" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.languages, "Languages")}
        </CvSection>
        <CvSection title="Achievements" className={styles.section} titleClassName={styles.sectionTitle}>
          {formatEntries(profile.achievements, "Achievements")}
        </CvSection>
        <footer className="cv-template-footer">
          Generated using NTT DATA Conversational CV Builder
        </footer>
      </motion.article>

      <div className={styles.actions}>
        <Button
          className="ntt-primary-button"
          appearance="primary"
          icon={<DocumentPdf20Regular />}
          disabled={disabled}
          onClick={onExportPdf}
        >
          Download PDF
        </Button>
        <Button
          className="ntt-secondary-button"
          appearance="secondary"
          icon={<ArrowDownload20Regular />}
          disabled={disabled}
          onClick={onExportDocx}
        >
          Download Word
        </Button>
      </div>
    </Card>
  );
}

function formatEntries(entries: string[], section: string): string {
  if (entries.length) return entries.join("\n\n");
  const missingContent: Record<string, string> = {
    Certifications: "Add certification details to complete your CV.",
    Experience: "Add professional experience details to complete your CV.",
    Projects: "Add relevant projects to complete your CV.",
    Education: "Add your education details to complete your CV.",
    Languages: "Add language details to complete your CV.",
    Achievements: "Add your achievements to complete your CV.",
  };
  return missingContent[section] ?? "Add details to complete your CV.";
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
    <section className={className}>
      <Text className={`ntt-cv-section-title ${titleClassName}`} size={200} weight="semibold">{title}</Text>
      <Text className="cv-section-content" size={200}>{children}</Text>
    </section>
  );
}
