import { useRef, useState } from "react";
import {
  Button,
  Card,
  Spinner,
  Text,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import {
  ArrowUpload20Regular,
  DocumentText20Regular,
  QuestionCircle20Regular,
  Sparkle20Regular,
} from "@fluentui/react-icons";
import { motion } from "framer-motion";
import { uploadDocument } from "../api/client";
import type { ResumeAnalysisItem } from "../types";

const useStyles = makeStyles({
  card: { padding: "20px" },
  dropzone: {
    display: "grid",
    justifyItems: "center",
    gap: "10px",
    padding: "24px 14px",
    border: `1px dashed ${tokens.colorNeutralStroke1}`,
    borderRadius: tokens.borderRadiusMedium,
    backgroundColor: tokens.colorNeutralBackground2,
    textAlign: "center",
  },
  icon: { color: tokens.colorBrandForeground1 },
  fileName: { overflowWrap: "anywhere" },
});

interface UploadPanelProps {
  fileName: string;
  extractionFailed: boolean;
  sourceText: string;
  analysis: ResumeAnalysisItem[];
  aiStatus: string;
  aiError: string;
  aiNextQuestion: string;
  suggestedImprovements: string[];
  generating: boolean;
  onUploaded: () => Promise<void>;
  onError: (message: string) => void;
  onExtractionFailed: (fileName: string) => void;
  onGenerate: () => void;
  onAskMissingQuestions: () => void;
}

export default function UploadPanel({
  fileName,
  extractionFailed,
  sourceText,
  analysis,
  aiStatus,
  aiError,
  aiNextQuestion,
  suggestedImprovements,
  generating,
  onUploaded,
  onError,
  onExtractionFailed,
  onGenerate,
  onAskMissingQuestions,
}: UploadPanelProps) {
  const styles = useStyles();
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);

  async function handleFile(file?: File) {
    if (!file) return;
    onError("");
    setBusy(true);
    try {
      await uploadDocument(file);
      await onUploaded();
    } catch (error) {
      onExtractionFailed(file.name);
      onError(error instanceof Error ? error.message : "The file could not be uploaded.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <Card className={styles.card}>
      <div className="panel-heading">
        <div>
          <Text weight="semibold" size={400}>Start with your resume</Text>
          <Text className="muted-text" size={200}>We’ll use it as a reference</Text>
        </div>
      </div>
      <div className={styles.dropzone}>
        {busy ? <Spinner size="small" /> : <ArrowUpload20Regular className={styles.icon} />}
        <Text weight="semibold" size={300}>Upload a document</Text>
        <Text className="muted-text" size={200}>PDF, DOCX, or PPTX · up to 10 MB</Text>
        <Button
          appearance="secondary"
          disabled={busy}
          icon={<ArrowUpload20Regular />}
          onClick={() => inputRef.current?.click()}
        >
          Browse files
        </Button>
        <input
          ref={inputRef}
          className="visually-hidden"
          type="file"
          accept=".pdf,.docx,.pptx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.openxmlformats-officedocument.presentationml.presentation"
          onChange={(event) => {
            void handleFile(event.target.files?.[0]);
            event.target.value = "";
          }}
        />
      </div>
      {fileName && (
        <motion.div
          className="uploaded-resume-details"
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <div className="uploaded-file">
            <DocumentText20Regular />
            <Text className={styles.fileName} size={200}>{fileName}</Text>
            <span
              className={`upload-success-mark${extractionFailed ? " upload-warning-mark" : ""}`}
              aria-label={extractionFailed ? "Extraction failed" : "Upload complete"}
            >
              {extractionFailed ? "!" : "✓"}
            </span>
          </div>
          <div className="resume-analysis">
            <div className="resume-analysis-heading">
              <div>
                <Text weight="semibold" size={300}>Resume Analysis</Text>
                <Text className="muted-text" size={200}>
                  {sourceText.trim() ? "Extracted details we can use" : "No text was extracted"}
                </Text>
              </div>
              <span className="analysis-count">{analysis.filter((item) => item.found).length}/{analysis.length}</span>
            </div>
            {aiError && (
              <Text className="extraction-fallback-note" size={200}>
                {aiError} You can continue with the extracted text and edit the CV in chat.
              </Text>
            )}
            {(aiStatus === "success" || aiStatus === "optimized") && (
              <Text className="muted-text" size={200}>Copilot analyzed the resume and extracted structured CV details.</Text>
            )}
            {aiNextQuestion && (aiStatus === "success" || aiStatus === "optimized") && (
              <Text className="muted-text" size={200}>Next question: {aiNextQuestion}</Text>
            )}
            {!sourceText.trim() && (
              <Text className="extraction-fallback-note" size={200}>
                We’ll use sample CV data so you can preview the NTT DATA template.
              </Text>
            )}
            <ul className="analysis-list">
              {analysis.map((item) => (
                <li className={item.found ? "analysis-found" : "analysis-missing"} key={item.label}>
                  <span aria-hidden="true">{item.found ? "✓" : "!"}</span>
                  {item.label} {item.found ? "found" : "missing"}
                </li>
              ))}
            </ul>
            {suggestedImprovements.length > 0 && (
              <div className="ai-suggestions">
                <Text weight="semibold" size={200}>Suggested improvements</Text>
                <ul>
                  {suggestedImprovements.map((suggestion) => <li key={suggestion}>{suggestion}</li>)}
                </ul>
              </div>
            )}
            <details className="source-preview">
              <summary>{sourceText.trim() ? "Review extracted text" : "Extraction unavailable"}</summary>
              <pre>{sourceText.slice(0, 4000)}
                {sourceText.length > 4000 ? "\n\n(Preview limited to 4,000 characters.)" : ""}
              </pre>
            </details>
          </div>
          <div className="upload-actions">
            <motion.div whileHover={{ y: -2 }} whileTap={{ scale: 0.98 }}>
              <Button
                className="ntt-primary-button"
                appearance="primary"
                icon={generating ? <Spinner size="tiny" /> : <Sparkle20Regular />}
                disabled={!fileName || (!sourceText.trim() && !extractionFailed) || generating}
                onClick={onGenerate}
              >
                {generating ? "Generating CV…" : "Generate NTT CV"}
              </Button>
            </motion.div>
            <Button
              className="ntt-secondary-button"
              appearance="subtle"
              icon={<QuestionCircle20Regular />}
              disabled={!fileName || generating || (analysis.every((item) => item.found) && !aiNextQuestion)}
              onClick={onAskMissingQuestions}
            >
              Ask Missing Questions
            </Button>
          </div>
        </motion.div>
      )}
    </Card>
  );
}
