import type { CvDesignConfiguration } from "../types";

export const DEFAULT_CV_DESIGN: CvDesignConfiguration = {
  id: "current",
  name: "Current design",
  primaryColor: "#00A9E0",
  secondaryColor: "#003B5C",
  lightColor: "#F5FBFF",
  fontFamily: "Arial",
  fontSize: 10,
  pageMargin: 42,
  sectionSpacing: 8,
  layout: "standard",
  showLogo: true,
  showFooter: true,
  sections: [
    { id: "summary", title: "Professional Summary", enabled: true, order: 0 },
    { id: "skills", title: "Technical Skills", enabled: true, order: 1 },
    { id: "experience", title: "Professional Experience", enabled: true, order: 2 },
    { id: "achievements", title: "Key Achievements", enabled: true, order: 3 },
    { id: "projects", title: "Projects", enabled: true, order: 4 },
    { id: "certifications", title: "Certifications", enabled: true, order: 5 },
    { id: "education", title: "Education", enabled: true, order: 6 },
    { id: "additional", title: "Additional Information", enabled: true, order: 7 },
  ],
  customSections: [],
};

export function cloneDesignConfiguration(configuration: CvDesignConfiguration): CvDesignConfiguration {
  return {
    ...configuration,
    sections: configuration.sections.map((section) => ({ ...section })),
    customSections: configuration.customSections.map((section) => ({ ...section })),
  };
}
