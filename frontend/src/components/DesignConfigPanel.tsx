import { useEffect, useState } from "react";
import {
  Button,
  Checkbox,
  Dropdown,
  Input,
  Option,
  Text,
  Textarea,
} from "@fluentui/react-components";
import { Add20Regular, ArrowDown20Regular, ArrowUp20Regular, Delete20Regular, Save20Regular } from "@fluentui/react-icons";
import type { CvDesignConfiguration } from "../types";

interface DesignConfigPanelProps {
  configuration: CvDesignConfiguration;
  presets: CvDesignConfiguration[];
  busy: boolean;
  onChange: (configuration: CvDesignConfiguration) => void;
  onSave: (name: string) => Promise<void>;
  onLoad: (configuration: CvDesignConfiguration) => void;
  onDelete: (id: string) => Promise<void>;
}

export default function DesignConfigPanel({
  configuration,
  presets,
  busy,
  onChange,
  onSave,
  onLoad,
  onDelete,
}: DesignConfigPanelProps) {
  const [presetName, setPresetName] = useState(configuration.name);

  useEffect(() => setPresetName(configuration.name), [configuration.id, configuration.name]);

  function update<K extends keyof CvDesignConfiguration>(
    key: K,
    value: CvDesignConfiguration[K],
  ) {
    onChange({ ...configuration, [key]: value });
  }

  function moveSection(index: number, direction: -1 | 1) {
    const ordered = [...configuration.sections].sort((a, b) => a.order - b.order);
    const nextIndex = index + direction;
    if (nextIndex < 0 || nextIndex >= ordered.length) return;
    [ordered[index], ordered[nextIndex]] = [ordered[nextIndex], ordered[index]];
    update("sections", ordered.map((section, order) => ({ ...section, order })));
  }

  return (
    <div className="design-config-panel">
      <div className="design-config-intro">
        <Text as="h2" size={500} weight="semibold">PDF template designer</Text>
        <Text className="muted-text" size={200}>
          Adjust the design and watch the CV preview update immediately. Saved presets are shared by everyone using this app.
        </Text>
      </div>

      <section className="design-config-group" aria-labelledby="preset-heading">
        <Text id="preset-heading" weight="semibold" size={300}>Shared configurations</Text>
        <div className="design-preset-save">
          <Input
            aria-label="Configuration name"
            value={presetName}
            maxLength={60}
            placeholder="Name this configuration"
            onChange={(_, data) => setPresetName(data.value)}
          />
          <Button
            appearance="primary"
            icon={<Save20Regular />}
            disabled={busy || !presetName.trim()}
            onClick={() => void onSave(presetName.trim())}
          >
            Save
          </Button>
        </div>
        {presets.length ? (
          <div className="design-preset-list">
            {presets.map((preset) => (
              <div className="design-preset-row" key={preset.id}>
                <Button appearance="subtle" disabled={busy} onClick={() => onLoad(preset)}>
                  {preset.name}
                </Button>
                <Button
                  appearance="subtle"
                  icon={<Delete20Regular />}
                  aria-label={`Delete ${preset.name} configuration`}
                  disabled={busy}
                  onClick={() => {
                    if (window.confirm(`Delete the shared "${preset.name}" configuration?`)) {
                      void onDelete(preset.id);
                    }
                  }}
                />
              </div>
            ))}
          </div>
        ) : (
          <Text className="muted-text" size={200}>No saved configurations yet.</Text>
        )}
        <Text className="design-config-note" size={100}>
          Presets store layout and styling. Custom section text stays with this CV and is not shared.
        </Text>
      </section>

      <section className="design-config-group" aria-labelledby="color-heading">
        <Text id="color-heading" weight="semibold" size={300}>Brand colors</Text>
        <div className="design-color-grid">
          <label>
            <span>Accent</span>
            <input className="design-color-input" aria-label="Accent color" type="color"
              value={configuration.primaryColor}
              onChange={(event) => update("primaryColor", event.currentTarget.value)} />
          </label>
          <label>
            <span>Text</span>
            <input className="design-color-input" aria-label="Text color" type="color"
              value={configuration.secondaryColor}
              onChange={(event) => update("secondaryColor", event.currentTarget.value)} />
          </label>
          <label>
            <span>Section background</span>
            <input className="design-color-input" aria-label="Section background color" type="color"
              value={configuration.lightColor}
              onChange={(event) => update("lightColor", event.currentTarget.value)} />
          </label>
        </div>
      </section>

      <section className="design-config-group" aria-labelledby="type-heading">
        <Text id="type-heading" weight="semibold" size={300}>Typography and layout</Text>
        <div className="design-control-grid">
          <label>
            <span>Font</span>
            <Dropdown
              aria-label="Font family"
              value={configuration.fontFamily}
              selectedOptions={[configuration.fontFamily]}
              onOptionSelect={(_, data) => {
                if (data.optionValue) update("fontFamily", data.optionValue);
              }}
            >
              {["Arial", "Aptos", "Georgia", "Times New Roman"].map((font) => (
                <Option key={font} value={font}>{font}</Option>
              ))}
            </Dropdown>
          </label>
          <label>
            <span>Font size</span>
            <Input type="number" min={8} max={14} value={String(configuration.fontSize)}
              onChange={(_, data) => update("fontSize", clamp(Number(data.value), 8, 14))} />
          </label>
          <label>
            <span>Page margin (pt)</span>
            <Input type="number" min={24} max={72} value={String(configuration.pageMargin)}
              onChange={(_, data) => update("pageMargin", clamp(Number(data.value), 24, 72))} />
          </label>
          <label>
            <span>Section spacing (pt)</span>
            <Input type="number" min={4} max={20} value={String(configuration.sectionSpacing)}
              onChange={(_, data) => update("sectionSpacing", clamp(Number(data.value), 4, 20))} />
          </label>
          <label>
            <span>Density</span>
            <Dropdown
              aria-label="Page layout density"
              value={configuration.layout === "compact" ? "Compact" : "Standard"}
              selectedOptions={[configuration.layout]}
              onOptionSelect={(_, data) => {
                if (data.optionValue === "compact" || data.optionValue === "standard") {
                  update("layout", data.optionValue);
                }
              }}
            >
              <Option value="standard">Standard</Option>
              <Option value="compact">Compact</Option>
            </Dropdown>
          </label>
        </div>
      </section>

      <section className="design-config-group" aria-labelledby="sections-heading">
        <Text id="sections-heading" weight="semibold" size={300}>Sections and branding</Text>
        <div className="design-section-list">
          {[...configuration.sections].sort((a, b) => a.order - b.order).map((section, index) => (
            <div className="design-section-row" key={section.id}>
              <Checkbox
                checked={section.enabled}
                label={section.title}
                onChange={(_, data) => update("sections", configuration.sections.map((item) =>
                  item.id === section.id ? { ...item, enabled: data.checked === true } : item))}
              />
              <div>
                <Button appearance="subtle" size="small" icon={<ArrowUp20Regular />}
                  aria-label={`Move ${section.title} up`} disabled={index === 0}
                  onClick={() => moveSection(index, -1)} />
                <Button appearance="subtle" size="small" icon={<ArrowDown20Regular />}
                  aria-label={`Move ${section.title} down`} disabled={index === configuration.sections.length - 1}
                  onClick={() => moveSection(index, 1)} />
              </div>
            </div>
          ))}
        </div>
        <Checkbox checked={configuration.showLogo} label="Show NTT DATA logo"
          onChange={(_, data) => update("showLogo", data.checked === true)} />
        <Checkbox checked={configuration.showFooter} label="Show CV footer and page numbers"
          onChange={(_, data) => update("showFooter", data.checked === true)} />
      </section>

      <section className="design-config-group" aria-labelledby="custom-heading">
        <div className="design-custom-heading">
          <Text id="custom-heading" weight="semibold" size={300}>Custom sections</Text>
          <Button appearance="secondary" size="small" icon={<Add20Regular />}
            disabled={configuration.customSections.length >= 8}
            onClick={() => update("customSections", [
              ...configuration.customSections,
              { id: crypto.randomUUID(), title: "New section", content: "" },
            ])}>
            Add section
          </Button>
        </div>
        {configuration.customSections.map((section) => (
          <div className="design-custom-section" key={section.id}>
            <div className="design-custom-fields">
              <Input aria-label="Custom section title" maxLength={60} value={section.title}
                onChange={(_, data) => update("customSections", configuration.customSections.map((item) =>
                  item.id === section.id ? { ...item, title: data.value } : item))} />
              <Button appearance="subtle" icon={<Delete20Regular />}
                aria-label={`Remove ${section.title} section`}
                onClick={() => update("customSections", configuration.customSections.filter((item) => item.id !== section.id))} />
            </div>
            <Textarea aria-label={`${section.title} section content`}
              placeholder="Add content for this CV only" value={section.content} maxLength={4000}
              resize="vertical" onChange={(_, data) => update("customSections", configuration.customSections.map((item) =>
                item.id === section.id ? { ...item, content: data.value } : item))} />
          </div>
        ))}
      </section>
    </div>
  );
}

function clamp(value: number, minimum: number, maximum: number): number {
  return Number.isFinite(value) ? Math.min(maximum, Math.max(minimum, value)) : minimum;
}
