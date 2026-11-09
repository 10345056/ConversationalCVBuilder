import type { CvProfile, UploadedDocument, Workspace } from "../types";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, init);
  if (!response.ok) {
    throw new Error("We couldn't complete your request. Please try again.");
  }
  return response.json() as Promise<T>;
}

export const getWorkspace = async () =>
  normalizeWorkspace(await request<Workspace>("/api/workspace"));

export const startNewSession = async () =>
  normalizeWorkspace(await request<Workspace>("/api/workspace/new-session", {
    method: "POST",
  }));

export const sendMessage = async (message: string) =>
  normalizeWorkspace(await request<Workspace>("/api/workspace/messages", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message }),
  }));

export const selectTemplate = async (templateId: string) =>
  normalizeWorkspace(await request<Workspace>("/api/workspace/template", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ templateId }),
  }));

export async function updateWorkspaceProfile(profile: CvProfile): Promise<Workspace> {
  const workspace = await request<Workspace>("/api/workspace/profile", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(profile),
  });
  return normalizeWorkspace(workspace);
}

export async function saveWorkspaceProfile(profile: CvProfile): Promise<Workspace> {
  const workspace = await request<Workspace>("/api/workspace/profile/manual", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(profile),
  });
  return normalizeWorkspace(workspace);
}

export async function uploadDocument(file: File): Promise<UploadedDocument> {
  const formData = new FormData();
  formData.append("file", file);
  return request<UploadedDocument>("/api/documents/parse", {
    method: "POST",
    body: formData,
  });
}

export async function downloadPdf(templateId: string): Promise<void> {
  await downloadExport("pdf", templateId);
}

export async function downloadDocx(templateId: string): Promise<void> {
  await downloadExport("docx", templateId);
}

async function downloadExport(format: "pdf" | "docx", templateId: string): Promise<void> {
  const response = await fetch(`${apiBaseUrl}/api/export/${format}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ templateKey: templateId }),
  });
  if (!response.ok) {
    throw new Error("Your CV could not be exported. Please try again.");
  }

  const url = URL.createObjectURL(await response.blob());
  const link = document.createElement("a");
  link.href = url;
  link.download = getFileName(response.headers.get("content-disposition"), format);
  link.style.display = "none";
  document.body.append(link);
  link.click();
  link.remove();
  window.setTimeout(() => URL.revokeObjectURL(url), 1000);
}

function normalizeWorkspace(workspace: Workspace): Workspace {
  const profile = workspace.profile;
  return {
    ...workspace,
    aiStatus: workspace.aiStatus ?? "not-run",
    aiError: workspace.aiError ?? "",
    aiNextQuestion: workspace.aiNextQuestion ?? "",
    aiTemplateRecommendation: workspace.aiTemplateRecommendation ?? "modern",
    aiMissingFields: workspace.aiMissingFields ?? [],
    aiSuggestedImprovements: workspace.aiSuggestedImprovements ?? [],
    profile: {
      ...profile,
      certifications: profile.certifications ?? profile.certificationLabels ?? [],
      projects: profile.projects ?? profile.projectSummaries ?? [],
      languages: profile.languages ?? profile.languageLabels ?? [],
      achievements: profile.achievements ?? [],
      additionalInformation: profile.additionalInformation ?? [],
      linkedIn: profile.linkedIn ?? "",
    },
  };
}

function getFileName(contentDisposition: string | null, format: "pdf" | "docx"): string {
  const fileName = contentDisposition?.match(/filename\*?=(?:UTF-8''|")?([^";\n]+)/i)?.[1];
  if (!fileName) return `ntt-data-cv.${format}`;
  try {
    return decodeURIComponent(fileName.replace(/^"|"$/g, ""));
  } catch {
    return `ntt-data-cv.${format}`;
  }
}
