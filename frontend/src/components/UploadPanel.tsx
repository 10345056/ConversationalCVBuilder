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
} from "@fluentui/react-icons";
import { motion } from "framer-motion";
import { uploadDocument } from "../api/client";
import type { ResumeAnalysisItem, UploadedDocument } from "../types";

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
  analysis: ResumeAnalysisItem[];
  targetRole: string;
  draftReady: boolean;
  generating: boolean;
  onUploaded: () => Promise<void>;
  onError: (message: string) => void;
  onExtractionFailed: (fileName: string) => void;
  onGenerate: () => void;
}

export default function UploadPanel({
  fileName,
  analysis,
  targetRole,
  draftReady,
  generating,
  onUploaded,
  onError,
  onExtractionFailed,
  onGenerate,
}: UploadPanelProps) {
  const styles = useStyles();
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);
  const [uploadResult, setUploadResult] = useState<UploadedDocument | null>(null);

  async function handleFile(file?: File) {
    if (!file) return;
    onError("");
    setUploadResult(null);
    setBusy(true);
    try {
      setUploadResult(await uploadDocument(file));
      await onUploaded();
    } catch {
      onExtractionFailed(file.name);
      onError("Your document could not be added. Please try again.");
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
        <Text weight="semibold" size={300}>Add your resume</Text>
        <Text className="muted-text" size={200}>Choose a PDF, Word, PowerPoint, or text file (up to 10 MB).</Text>
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
          accept=".pdf,.docx,.pptx,.txt,application/pdf,text/plain,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.openxmlformats-officedocument.presentationml.presentation"
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
              className="upload-success-mark"
              aria-label={uploadResult ? "Document uploaded" : "Document selected"}
            >
              ✓
            </span>
          </div>
          <div className="resume-analysis">
            <div className="resume-analysis-heading">
              <div>
                <Text weight="semibold" size={300}>Resume Review</Text>
                <Text className="muted-text" size={200}>
                  Review your information before exporting.
                </Text>
              </div>
            </div>
            <Text className="resume-status-message" size={200}>
              {draftReady
                ? "Your resume information has been organized into the NTT DATA Professional CV format. Please review the generated content and update any details where necessary."
                : "Your resume is ready to review. Prepare a CV draft to update the live preview."}
            </Text>
            <ul className="analysis-list">
              <li>Document status: {uploadResult ? "Uploaded" : "Selected"}</li>
              <li>Target role: {targetRole || uploadResult?.profile?.targetRole || "Please review"}</li>
            </ul>
            <ul className="analysis-list">
              {analysis.map((item) => (
                <li className={item.found ? "analysis-found" : "analysis-missing"} key={item.label}>
                  <span aria-hidden="true">{item.found ? "✓" : "!"}</span>
                  {item.label}: {item.found ? "Available" : "Needs review"}
                </li>
              ))}
            </ul>
          </div>
          <div className="upload-actions">
            <motion.div whileHover={{ y: -2 }} whileTap={{ scale: 0.98 }}>
              <Button
                className="ntt-primary-button"
                appearance="primary"
                icon={generating ? <Spinner size="tiny" /> : <DocumentText20Regular />}
                disabled={!fileName || generating}
                onClick={onGenerate}
              >
                {generating ? "Preparing your CV…" : draftReady ? "Update NTT CV" : "Generate NTT CV"}
              </Button>
            </motion.div>
          </div>
        </motion.div>
      )}
    </Card>
  );
}
