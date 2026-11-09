import { useCallback, useEffect, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import {
  Button,
  MessageBar,
  MessageBarBody,
  Text,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { Sparkle20Regular } from "@fluentui/react-icons";
import {
  downloadPdf,
  downloadDocx,
  getWorkspace,
  sendMessage,
  saveWorkspaceProfile,
  startNewSession,
  updateWorkspaceProfile,
} from "./api/client";
import ConversationPanel from "./components/ConversationPanel";
import CvPreview from "./components/CvPreview";
import UploadPanel from "./components/UploadPanel";
import { hasProfileContent, hasProfileItems, parseResumeText } from "./utils/resumeParser";
import type { ResumeAnalysisItem, Workspace } from "./types";

const useStyles = makeStyles({
  app: {
    minHeight: "100vh",
    color: tokens.colorNeutralForeground1,
    backgroundColor: "#f5f7fb",
  },
});

export default function App() {
  const styles = useStyles();
  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [hasEntered, setHasEntered] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [startingNewSession, setStartingNewSession] = useState(false);
  const [fallbackFileName, setFallbackFileName] = useState("");
  const [sessionKey, setSessionKey] = useState(0);
  const [draftReady, setDraftReady] = useState(false);
  const [savingProfile, setSavingProfile] = useState(false);

  const refreshWorkspace = useCallback(async () => {
    setWorkspace(await getWorkspace());
  }, []);

  useEffect(() => {
    void startNewSession()
      .then(setWorkspace)
      .catch((cause: unknown) => {
        setError(cause instanceof TypeError
            ? "The CV workspace is temporarily unavailable. Please try again shortly."
            : "The CV workspace could not be opened. Please try again.");
      })
      .finally(() => setLoading(false));
  }, []);

  async function handleStartNewSession() {
    setError("");
    setStartingNewSession(true);
    try {
      setWorkspace(await startNewSession());
      setFallbackFileName("");
      setDraftReady(false);
      setSessionKey((current) => current + 1);
    } catch {
      setError("A new CV could not be started. Please try again.");
    } finally {
      setStartingNewSession(false);
    }
  }

  async function handleSend(message: string) {
    try {
      setError("");
      setWorkspace(await sendMessage(message));
    } catch {
      setError("Your CV details could not be saved. Please try again.");
    }
  }

  async function handleGenerateCv() {
    const fileName = fallbackFileName || workspace?.uploadedDocumentName;
    if (!workspace || !fileName) return;
    setError("");
    setGenerating(true);
    try {
      const sourceText = fallbackFileName ? "" : workspace.sourceText;
      const { profile: parsedProfile } = parseResumeText(sourceText);
      const currentProfile = workspace.profile.fullName.trim()
        && workspace.profile.fullName !== "CV Builder User"
        ? workspace.profile
        : parsedProfile;
      const profile = mergeProfiles(currentProfile, parsedProfile);
      const updatedWorkspace = await updateWorkspaceProfile(profile);
      setWorkspace({ ...updatedWorkspace, uploadedDocumentName: fileName, sourceText });
      setDraftReady(true);
    } catch {
      setError("Your CV draft could not be prepared. Please try again.");
    } finally {
      setGenerating(false);
    }
  }

  async function handleSaveProfile(profile: Workspace["profile"]): Promise<boolean> {
    setError("");
    setSavingProfile(true);
    try {
      const updatedWorkspace = await saveWorkspaceProfile(profile);
      setWorkspace(updatedWorkspace);
      setDraftReady(true);
      return true;
    } catch {
      setError("Your CV changes could not be saved. Please try again.");
      return false;
    } finally {
      setSavingProfile(false);
    }
  }

  async function handleExportDocx() {
    try {
      setError("");
      setExporting(true);
      await downloadDocx(workspace?.selectedTemplateId ?? "classic");
    } catch {
      setError("Your Word document could not be exported. Please try again.");
    } finally {
      setExporting(false);
    }
  }

  async function handleExportPdf() {
    try {
      setError("");
      setExporting(true);
      await downloadPdf(workspace?.selectedTemplateId ?? "classic");
    } catch {
      setError("Your PDF could not be exported. Please try again.");
    } finally {
      setExporting(false);
    }
  }

  return (
    <AnimatePresence mode="wait" initial={false}>
      {!hasEntered ? (
        <WelcomePage key="welcome" onStart={() => setHasEntered(true)} />
      ) : (
    <motion.div
      key="workspace"
      className={styles.app}
      initial={{ opacity: 0, y: 22, scale: 0.985 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      transition={{ duration: 0.55, ease: [0.22, 1, 0.36, 1] }}
    >
      {loading ? (
        <main className={`${styles.app} loading-screen`}>Loading your workspace…</main>
      ) : (
      <>
      <header className="app-header">
        <div className="brand">
          <img className="app-brand-logo" src="/assets/ntt-logo.png" alt="NTT DATA" />
          <div>
            <Text className="brand-subtitle" size={200}>Professional CV Builder</Text>
          </div>
        </div>
        <div className="header-note">
          <span className="online-dot" />
          <Text size={200}>NTT DATA Professional CV</Text>
        </div>
        <Button
          appearance="secondary"
          size="small"
          className="new-session-button"
          disabled={loading || startingNewSession || generating || exporting}
          onClick={() => void handleStartNewSession()}
        >
          {startingNewSession ? "Starting…" : "Start new session"}
        </Button>
      </header>

      <main className="workspace">
        <div className="workspace-intro">
          <div>
            <Text as="h1" size={700} weight="bold">Create your professional CV.</Text>
            <Text className="muted-text" size={300}>
                Organize your experience into a clear, professional format.
            </Text>
          </div>
            <div className="step-count"><span>01</span> / 03 <span className="step-label">Prepare · Review · Export</span></div>
        </div>

        {error && (
          <MessageBar intent="error" className="app-error">
            <MessageBarBody>{error}</MessageBarBody>
          </MessageBar>
        )}
        {draftReady && (
          <MessageBar intent="success" className="app-success">
            <MessageBarBody>
              <Text weight="semibold">CV Draft Ready</Text>
              <br />
              Your resume has been processed successfully and the live preview has been updated. Review the generated content and make any required changes before exporting.
            </MessageBarBody>
          </MessageBar>
        )}
        {workspace ? (
          <section className="three-panel-layout" aria-label="CV builder workspace">
            <aside className="upload-column" aria-label="Upload section">
              <UploadPanel
                key={sessionKey}
                fileName={fallbackFileName || workspace.uploadedDocumentName}
                analysis={getWorkspaceAnalysis(workspace, Boolean(fallbackFileName))}
                targetRole={workspace.profile.targetRole}
                draftReady={draftReady}
                generating={generating}
                onUploaded={async () => {
                  setFallbackFileName("");
                  setDraftReady(false);
                  await refreshWorkspace();
                }}
                onError={setError}
                onExtractionFailed={(fileName) => {
                  setFallbackFileName(fileName);
                  setDraftReady(false);
                }}
                onGenerate={() => void handleGenerateCv()}
              />
            </aside>

            <section className="conversation-column" aria-label="Additional CV details">
              <ConversationPanel
                key={sessionKey}
                messages={workspace.messages}
                disabled={false}
                profile={workspace.profile}
                onSend={handleSend}
                onSaveProfile={handleSaveProfile}
                onError={setError}
              />
            </section>

            <aside className="preview-column" aria-label="Live CV preview">
              <CvPreview
                profile={workspace.profile}
                templateId={workspace.selectedTemplateId}
                onExportDocx={() => void handleExportDocx()}
                onExportPdf={() => void handleExportPdf()}
                onSave={handleSaveProfile}
                saving={savingProfile}
                disabled={exporting || generating || savingProfile}
              />
            </aside>
          </section>
        ) : (
          <div className="connection-state">
            <Text weight="semibold" size={400}>Your workspace is not available yet.</Text>
            <Text className="muted-text" size={300}>Please refresh the page or try again later.</Text>
          </div>
        )}
      </main>
      <footer className="app-footer">
        <Text size={200}>Your information is used to prepare your professional CV.</Text>
      </footer>
      </>
      )}
    </motion.div>
      )}
    </AnimatePresence>
  );
}

function mergeProfiles(primary: Workspace["profile"], fallback: Workspace["profile"]): Workspace["profile"] {
  const mergeText = (preferred: string | undefined, secondary: string | undefined) =>
    preferred?.trim() && preferred !== "Your Name" && preferred !== "Role to confirm"
      ? preferred
      : secondary?.trim() || preferred || "";
  const mergeItems = (preferred: string[] | undefined, secondary: string[] | undefined) =>
    [...(preferred ?? []), ...(secondary ?? [])]
      .filter((item, index, values) => item.trim()
        && values.findIndex((value) => value.trim().toLowerCase() === item.trim().toLowerCase()) === index);

  return {
    ...primary,
    fullName: mergeText(primary.fullName, fallback.fullName),
    targetRole: mergeText(primary.targetRole, fallback.targetRole),
    email: mergeText(primary.email, fallback.email),
    phone: mergeText(primary.phone, fallback.phone),
    location: mergeText(primary.location, fallback.location),
    linkedIn: mergeText(primary.linkedIn, fallback.linkedIn),
    summary: mergeText(primary.summary, fallback.summary),
    skills: mergeItems(primary.skills, fallback.skills),
    experience: mergeItems(primary.experience, fallback.experience),
    education: mergeItems(primary.education, fallback.education),
    certifications: mergeItems(primary.certifications, fallback.certifications),
    projects: mergeItems(primary.projects, fallback.projects),
    languages: mergeItems(primary.languages, fallback.languages),
    achievements: mergeItems(primary.achievements, fallback.achievements),
    additionalInformation: mergeItems(primary.additionalInformation, fallback.additionalInformation),
  };
}

function getAnalysisProfile(workspace: Workspace, extractionFailed = false) {
  if (extractionFailed) {
    return {
      ...workspace.profile,
      fullName: "",
      targetRole: "",
      email: "",
      phone: "",
      location: "",
      summary: "",
      skills: [],
      experience: [],
      education: [],
      certifications: [],
      projects: [],
      languages: [],
      achievements: [],
      additionalInformation: [],
    };
  }
  if (!workspace.uploadedDocumentName) return workspace.profile;
  if (workspace.aiStatus === "success" || workspace.aiStatus === "optimized") {
    return workspace.profile;
  }
  if (workspace.aiStatus === "fallback") return workspace.profile;
  if (!extractionFailed && workspace.currentStep === "complete") return workspace.profile;
  if (workspace.sourceText.trim()) return parseResumeText(workspace.sourceText).profile;
  if (workspace.profile.fullName !== "CV Builder User") return workspace.profile;
  return {
    ...workspace.profile,
    fullName: "",
    targetRole: "",
    email: "",
    phone: "",
    location: "",
  };
}

function getWorkspaceAnalysis(workspace: Workspace, extractionFailed = false): ResumeAnalysisItem[] {
  const profile = getAnalysisProfile(workspace, extractionFailed);
  const analysis: ResumeAnalysisItem[] = [
    { label: "Personal Details", found: hasProfileContent(profile.fullName) && (hasProfileContent(profile.email) || hasProfileContent(profile.phone)) },
    { label: "Summary", found: hasProfileContent(profile.summary) },
    { label: "Skills", found: hasProfileItems(profile.skills) },
    { label: "Experience", found: hasProfileItems(profile.experience) },
    { label: "Projects", found: hasProfileItems(profile.projects) },
    { label: "Education", found: hasProfileItems(profile.education) },
    { label: "Certifications", found: hasProfileItems(profile.certifications) },
    { label: "Languages", found: hasProfileItems(profile.languages) },
  ];
  if (extractionFailed || !["success", "optimized"].includes(workspace.aiStatus)) return analysis;

  const missing = workspace.aiMissingFields.map((field) => field.toLowerCase());
  if (missing.length === 0) return analysis;
  const isMissing = (label: string, ...terms: string[]) =>
    missing.some((field) => terms.some((term) => field.includes(term)))
      ? false
      : analysis.find((item) => item.label === label)?.found ?? false;
  return analysis.map((item) => {
    switch (item.label) {
      case "Personal Details": return { ...item, found: isMissing(item.label, "personal detail", "contact", "name") };
      case "Summary": return { ...item, found: isMissing(item.label, "summary") };
      case "Skills": return { ...item, found: isMissing(item.label, "skill") };
      case "Experience": return { ...item, found: isMissing(item.label, "experience", "employment") };
      case "Projects": return { ...item, found: isMissing(item.label, "project") };
      case "Education": return { ...item, found: isMissing(item.label, "education") };
      case "Certifications": return { ...item, found: isMissing(item.label, "certification", "license") };
      case "Languages": return { ...item, found: isMissing(item.label, "language") };
      default: return item;
    }
  });
}

function WelcomePage({ onStart }: { onStart: () => void }) {
  return (
    <motion.main
      className="welcome-screen"
      initial={{ opacity: 0, scale: 1.02 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 1.04, filter: "blur(10px)" }}
      transition={{ duration: 0.65, ease: [0.22, 1, 0.36, 1] }}
    >
      <div className="welcome-background" aria-hidden="true">
        <motion.div
          className="welcome-orb welcome-orb-one"
          animate={{ x: [0, 26, 0], y: [0, -22, 0], scale: [1, 1.08, 1] }}
          transition={{ duration: 12, repeat: Infinity, ease: "easeInOut" }}
        />
        <motion.div
          className="welcome-orb welcome-orb-two"
          animate={{ x: [0, -24, 0], y: [0, 20, 0] }}
          transition={{ duration: 14, repeat: Infinity, ease: "easeInOut" }}
        />
        {Array.from({ length: 28 }, (_, index) => (
          <motion.span
            className="welcome-particle"
            key={index}
            style={{
              left: `${(index * 37 + 11) % 100}%`,
              top: `${(index * 61 + 7) % 100}%`,
              width: `${index % 5 === 0 ? 4 : 2}px`,
              height: `${index % 5 === 0 ? 4 : 2}px`,
            }}
            animate={{ y: [0, -24 - (index % 4) * 8, 0], opacity: [0.2, 0.8, 0.2] }}
            transition={{
              duration: 4 + (index % 5),
              delay: (index % 7) * 0.35,
              repeat: Infinity,
              ease: "easeInOut",
            }}
          />
        ))}
      </div>

      <header className="welcome-header">
        <a className="welcome-brand" href="#" aria-label="NTT DATA home">
          <span className="welcome-brand-mark" aria-hidden="true">
            
          </span>
          <span>NTT DATA</span>
          <span className="welcome-brand-divider" />
          <span className="welcome-brand-product">CV Builder</span>
        </a>
        <div className="welcome-header-status">
          <span className="welcome-status-dot" />
          PROFESSIONAL CV PREPARATION
        </div>
      </header>

      <section className="welcome-content">
        <motion.div
          className="welcome-copy"
          initial="hidden"
          animate="visible"
          variants={{
            hidden: {},
            visible: { transition: { staggerChildren: 0.12, delayChildren: 0.12 } },
          }}
        >
          <motion.div
            className="welcome-eyebrow"
            variants={{ hidden: { opacity: 0, y: 14 }, visible: { opacity: 1, y: 0 } }}
            transition={{ duration: 0.55 }}
          >
            <Sparkle20Regular />
            <span>YOUR NEXT CHAPTER STARTS HERE</span>
          </motion.div>
          <motion.h1
            variants={{ hidden: { opacity: 0, y: 22 }, visible: { opacity: 1, y: 0 } }}
            transition={{ duration: 0.65 }}
          >
            NTT DATA
            <span className="welcome-title-line">Professional</span>
            <span className="welcome-title-accent">CV Builder<span className="welcome-period">.</span></span>
          </motion.h1>
          <motion.p
            className="welcome-subtitle"
            variants={{ hidden: { opacity: 0, y: 18 }, visible: { opacity: 1, y: 0 } }}
            transition={{ duration: 0.6 }}
          >
            Prepare a clear, consistent CV using the NTT DATA professional format.
          </motion.p>
          <motion.div
            className="welcome-actions"
            variants={{ hidden: { opacity: 0, y: 16 }, visible: { opacity: 1, y: 0 } }}
            transition={{ duration: 0.6 }}
          >
            <motion.button
              className="welcome-start-button"
              onClick={onStart}
              whileHover={{ y: -3, scale: 1.025 }}
              whileTap={{ scale: 0.98 }}
              transition={{ type: "spring", stiffness: 400, damping: 22 }}
            >
              <span>Start building</span>
              <span className="welcome-button-arrow" aria-hidden="true">→</span>
            </motion.button>
            <span className="welcome-action-note">A polished CV starts with your experience</span>
          </motion.div>
          <motion.div
            className="welcome-benefits"
            variants={{ hidden: { opacity: 0 }, visible: { opacity: 1 } }}
            transition={{ duration: 0.7 }}
          >
            <span><i>01</i> Add your resume</span>
            <span><i>02</i> Review your details</span>
            <span><i>03</i> Export your CV</span>
          </motion.div>
        </motion.div>

        <motion.div
          className="welcome-visual"
          initial={{ opacity: 0, x: 50, rotate: 2 }}
          animate={{ opacity: 1, x: 0, rotate: 0 }}
          transition={{ duration: 0.9, delay: 0.35, ease: [0.22, 1, 0.36, 1] }}
        >
          <motion.div
            className="welcome-preview-card"
            animate={{ y: [0, -10, 0] }}
            transition={{ duration: 6, repeat: Infinity, ease: "easeInOut" }}
          >
            <div className="preview-card-topline">
              <span className="preview-window-dots"><i /><i /><i /></span>
              <span>YOUR PROFILE</span>
              <span className="preview-card-menu">···</span>
            </div>
            <div className="preview-profile">
              <div className="preview-avatar">JD</div>
              <div className="preview-profile-info">
                <strong>Jordan Davis</strong>
                <span>Senior Product Designer</span>
              </div>
              <span className="preview-ai-badge">Review ready</span>
            </div>
            <div className="preview-rule" />
            <div className="preview-section-title">
              <span>Professional summary</span>
              <span>✦</span>
            </div>
            <div className="preview-copy-line preview-copy-long" />
            <div className="preview-copy-line" />
            <div className="preview-copy-line preview-copy-mid" />
            <div className="preview-skills-heading">KEY STRENGTHS</div>
            <div className="preview-skills">
              <span>Product strategy</span><span>UX research</span><span>Leadership</span>
            </div>
            <div className="preview-progress">
              <span>Profile strength</span><strong>92%</strong>
              <div><i /></div>
            </div>
          </motion.div>
          <div className="welcome-visual-glow" aria-hidden="true" />
        </motion.div>
      </section>

      <footer className="welcome-footer">
        <span>Built for the people behind the possibilities.</span>
        <span><i /> Professional tools for clear, confident CVs</span>
      </footer>
    </motion.main>
  );
}
