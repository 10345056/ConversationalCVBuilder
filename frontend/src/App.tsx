import { useCallback, useEffect, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import {
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
  updateWorkspaceProfile,
} from "./api/client";
import ConversationPanel from "./components/ConversationPanel";
import CvPreview from "./components/CvPreview";
import UploadPanel from "./components/UploadPanel";
import { parseResumeText } from "./utils/resumeParser";
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
  const [usedSampleData, setUsedSampleData] = useState(false);
  const [fallbackFileName, setFallbackFileName] = useState("");

  const refreshWorkspace = useCallback(async () => {
    setWorkspace(await getWorkspace());
  }, []);

  useEffect(() => {
    void refreshWorkspace()
      .catch((cause: unknown) => {
        setError(cause instanceof TypeError
          ? "Could not connect to the API. Make sure it is running at https://localhost:5001."
          : cause instanceof Error ? cause.message : "Could not load the workspace.");
      })
      .finally(() => setLoading(false));
  }, [refreshWorkspace]);

  async function handleSend(message: string) {
    try {
      setError("");
      setWorkspace(await sendMessage(message));
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "The message could not be sent.");
    }
  }

  async function handleGenerateCv() {
    const fileName = fallbackFileName || workspace?.uploadedDocumentName;
    if (!workspace || !fileName) return;
    setError("");
    setGenerating(true);
    try {
      const sourceText = fallbackFileName ? "" : workspace.sourceText;
      const { profile: parsedProfile, usedSampleData: usedFallback } = parseResumeText(sourceText);
      const currentProfile = workspace.profile.fullName.trim()
        && workspace.profile.fullName !== "CV Builder User"
        ? workspace.profile
        : parsedProfile;
      const profile = mergeProfiles(currentProfile, parsedProfile);
      const updatedWorkspace = await updateWorkspaceProfile(profile);
      setWorkspace({ ...updatedWorkspace, uploadedDocumentName: fileName, sourceText });
      setUsedSampleData(
        !["success", "optimized"].includes(workspace.aiStatus) &&
        workspace.currentStep !== "complete" &&
        usedFallback,
      );
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "The NTT CV could not be generated.");
    } finally {
      setGenerating(false);
    }
  }

  async function handleAskMissingQuestions() {
    if (!workspace) return;
    const missing = getWorkspaceAnalysis(workspace, Boolean(fallbackFileName))
      .filter((item) => !item.found)
      .map((item) => item.label.toLowerCase());
    if (missing.length === 0 && !workspace.aiNextQuestion) return;
    const focus = workspace.aiMissingFields.length
      ? workspace.aiMissingFields.join(", ")
      : missing.length > 0
        ? missing.join(", ")
        : workspace.aiNextQuestion;
    await handleSend(`Continue the CV gap analysis. Ask me one focused question about ${focus}. Ask only one question and do not repeat any previous question.`);
  }

  async function handleExportDocx() {
    try {
      setError("");
      setExporting(true);
      await downloadDocx(workspace?.selectedTemplateId ?? "classic");
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "DOCX export failed.");
    } finally {
      setExporting(false);
    }
  }

  async function handleExportPdf() {
    try {
      setError("");
      setExporting(true);
      await downloadPdf(workspace?.selectedTemplateId ?? "classic");
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "PDF export failed.");
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
            <Text className="brand-subtitle" size={200}>Conversational CV Builder</Text>
          </div>
        </div>
        <div className="header-note">
          <span className="online-dot" />
          <Text size={200}>Your next chapter starts here</Text>
        </div>
      </header>

      <main className="workspace">
        <div className="workspace-intro">
          <div>
            <Text as="h1" size={700} weight="bold">Create a CV that feels like you.</Text>
            <Text className="muted-text" size={300}>
              Share your story, shape your draft, and make it yours.
            </Text>
          </div>
          <div className="step-count"><span>01</span> / 03 <span className="step-label">Build · Refine · Export</span></div>
        </div>

        {error && (
          <MessageBar intent="error" className="app-error">
            <MessageBarBody>{error}</MessageBarBody>
          </MessageBar>
        )}
        {usedSampleData && workspace?.sourceText.trim() && (
          <MessageBar intent="warning" className="app-error">
            <MessageBarBody>
              The uploaded text did not contain a recognizable candidate name, so sample CV data is shown in the preview.
            </MessageBarBody>
          </MessageBar>
        )}

        {workspace ? (
          <section className="three-panel-layout" aria-label="CV builder workspace">
            <aside className="upload-column" aria-label="Upload section">
              <UploadPanel
                fileName={fallbackFileName || workspace.uploadedDocumentName}
                extractionFailed={Boolean(fallbackFileName)}
                sourceText={fallbackFileName ? "" : workspace.sourceText}
                analysis={getWorkspaceAnalysis(workspace, Boolean(fallbackFileName))}
                aiStatus={workspace.aiStatus}
                aiError={workspace.aiError}
                aiNextQuestion={workspace.aiNextQuestion}
                suggestedImprovements={workspace.aiSuggestedImprovements}
                generating={generating}
                onUploaded={async () => {
                  setFallbackFileName("");
                  setUsedSampleData(false);
                  await refreshWorkspace();
                }}
                onError={setError}
                onExtractionFailed={setFallbackFileName}
                onGenerate={() => void handleGenerateCv()}
                onAskMissingQuestions={() => void handleAskMissingQuestions()}
              />
            </aside>

            <section className="conversation-column" aria-label="Conversational chat section">
              <ConversationPanel
                messages={workspace.messages}
                disabled={false}
                onSend={handleSend}
                onError={setError}
              />
            </section>

            <aside className="preview-column" aria-label="Live CV preview">
              <CvPreview
                profile={workspace.profile}
                templateId={workspace.selectedTemplateId}
                onExportDocx={() => void handleExportDocx()}
                onExportPdf={() => void handleExportPdf()}
                disabled={exporting || generating}
              />
            </aside>
          </section>
        ) : (
          <div className="connection-state">
            <Text weight="semibold" size={400}>Your workspace is not available yet.</Text>
            <Text className="muted-text" size={300}>Start the backend API, then refresh this page.</Text>
          </div>
        )}
      </main>
      <footer className="app-footer">
        <Text size={200}>Your information stays in this local MVP session.</Text>
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
    preferred?.trim() ? preferred : secondary ?? "";
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
    };
  }
  if (!workspace.uploadedDocumentName) return workspace.profile;
  if (workspace.aiStatus === "success" || workspace.aiStatus === "optimized") {
    return workspace.profile;
  }
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
    { label: "Personal Details", found: Boolean(profile.fullName.trim() && (profile.email.trim() || profile.phone.trim())) },
    { label: "Summary", found: Boolean(profile.summary.trim()) },
    { label: "Skills", found: profile.skills.length > 0 },
    { label: "Experience", found: profile.experience.length > 0 },
    { label: "Projects", found: profile.projects.length > 0 },
    { label: "Education", found: profile.education.length > 0 },
    { label: "Certifications", found: profile.certifications.length > 0 },
    { label: "Languages", found: profile.languages.length > 0 },
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
          AI-powered career storytelling
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
            <span className="welcome-title-line">Conversational</span>
            <span className="welcome-title-accent">CV Builder<span className="welcome-period">.</span></span>
          </motion.h1>
          <motion.p
            className="welcome-subtitle"
            variants={{ hidden: { opacity: 0, y: 18 }, visible: { opacity: 1, y: 0 } }}
            transition={{ duration: 0.6 }}
          >
            Create customer-ready resumes with AI assistance.
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
            <span className="welcome-action-note">A better CV begins with a conversation</span>
          </motion.div>
          <motion.div
            className="welcome-benefits"
            variants={{ hidden: { opacity: 0 }, visible: { opacity: 1 } }}
            transition={{ duration: 0.7 }}
          >
            <span><i>01</i> Tell your story</span>
            <span><i>02</i> Shape it with AI</span>
            <span><i>03</i> Make it yours</span>
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
              <span className="preview-ai-badge"><Sparkle20Regular /> AI refined</span>
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
        <span><i /> Thoughtful by design&nbsp; · &nbsp;Powered by AI</span>
      </footer>
    </motion.main>
  );
}
