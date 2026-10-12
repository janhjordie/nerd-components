import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

const API_DASHBOARD = "/umbraco/management/api/v1/the-nerd-collective/brevo/dashboard";

const TABS = [
  { id: "overview", label: "Oversigt" },
  { id: "contacts", label: "Kontakter" },
  { id: "lists", label: "Lister" },
  { id: "segments", label: "Segmenter" },
  { id: "campaigns", label: "Kampagner" },
];

export class TncBrevoDashboardElement extends UmbLitElement {
  static properties = {
    _loading: { state: true },
    _dashboard: { state: true },
    _error: { state: true },
    _tab: { state: true },
  };

  constructor() {
    super();
    this._loading = true;
    this._dashboard = null;
    this._error = null;
    this._tab = "overview";
  }

  connectedCallback() {
    super.connectedCallback();
    this.#load();
  }

  async #load() {
    this._loading = true;
    this._error = null;
    try {
      const { data, error } = await tryExecute(
        this,
        umbHttpClient.get({
          security: [{ scheme: "bearer", type: "http" }],
          url: API_DASHBOARD,
        }),
      );

      if (error) {
        this._error = error.message ?? `HTTP ${error.status ?? "?"}`;
        this._dashboard = null;
        return;
      }

      this._dashboard = data;
    } catch (err) {
      this._error = err?.message ?? "Kunne ikke hente Brevo-data";
      this._dashboard = null;
    } finally {
      this._loading = false;
    }
  }

  #setTab(tabId) {
    this._tab = tabId;
  }

  #formatDate(value) {
    if (!value) return "—";
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleString("da-DK");
  }

  #contactName(contact) {
    const parts = [contact?.firstName, contact?.lastName].filter(Boolean);
    return parts.length > 0 ? parts.join(" ") : "—";
  }

  render() {
    if (this._loading) {
      return html`
        <div class="layout">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    if (this._error) {
      return html`
        <div class="layout">
          <uui-box>
            <p><strong>Fejl:</strong> ${this._error}</p>
            <uui-button look="primary" label="Prøv igen" @click=${() => this.#load()}></uui-button>
          </uui-box>
        </div>
      `;
    }

    const d = this._dashboard;
    if (!d) {
      return nothing;
    }

    return html`
      <umb-body-layout class="layout">
        <div slot="header" class="header">
          <uui-tab-group>
            ${TABS.map(
              (tab) => html`
                <uui-tab
                  label=${tab.label}
                  ?active=${this._tab === tab.id}
                  @click=${() => this.#setTab(tab.id)}></uui-tab>
              `,
            )}
          </uui-tab-group>
          <uui-button look="secondary" label="Opdater" @click=${() => this.#load()}></uui-button>
        </div>

        <div class="content uui-text">
          ${d.dataError
            ? html`<uui-box class="warn-box">
                <p><strong>Brevo API:</strong> ${d.dataError}</p>
              </uui-box>`
            : ""}
          ${this.#renderTab(d)}
        </div>
      </umb-body-layout>
    `;
  }

  #renderTab(d) {
    switch (this._tab) {
      case "contacts":
        return this.#renderContacts(d.contacts);
      case "lists":
        return this.#renderLists(d.lists);
      case "segments":
        return this.#renderSegments(d.segments);
      case "campaigns":
        return this.#renderCampaigns(d.campaigns);
      default:
        return this.#renderOverview(d.status);
    }
  }

  #renderOverview(status) {
    const s = status;
    const cms = s?.cmsSettings;
    const hint = s?.settingsLocationHint ?? "Content";
    return html`
      <uui-box headline="Brevo-forbindelse">
        <p>
          API-nøgle konfigureres på serveren (<code>Brevo:ApiKey</code>). Indstillinger redigeres under
          <strong>${hint}</strong>.
        </p>
        ${s?.connected
          ? html`<p class="ok">
              Forbindelse OK — ${s.accountEmail ?? ""} · plan: ${s.planType ?? "—"} · ca.
              ${s.contactCount ?? "?"} kontakter i alt
            </p>`
          : html`<p class="bad">
              ${s?.apiKeyConfigured ? "Kunne ikke forbinde" : "Ingen API-nøgle"}${s?.error ? `: ${s.error}` : ""}
            </p>`}
      </uui-box>
      ${cms
        ? html`
            <uui-box headline="CMS-indstillinger (publiceret)">
              <ul class="meta-list">
                <li>Newsletter list ID: ${cms.newsletterListId}</li>
                <li>Sender: ${cms.senderName} &lt;${cms.senderEmail}&gt;</li>
                <li>Notify: ${cms.notifyEmail}</li>
              </ul>
            </uui-box>
          `
        : ""}
    `;
  }

  #renderContacts(page) {
    return html`
      <uui-box headline="Kontakter">
        <p class="intro">${page ? `${page.count} kontakter i Brevo (viser op til ${page.items?.length ?? 0})` : "Ingen data"}</p>
        ${this.#renderContactTable(page?.items)}
      </uui-box>
    `;
  }

  #renderLists(page) {
    return html`
      <uui-box headline="Lister">
        <p class="intro">${page ? `${page.count} lister i Brevo` : "Ingen data"}</p>
        ${page?.items?.length
          ? html`
              <uui-table>
                <uui-table-head>
                  <uui-table-head-cell>ID</uui-table-head-cell>
                  <uui-table-head-cell>Navn</uui-table-head-cell>
                  <uui-table-head-cell>Abonnenter</uui-table-head-cell>
                </uui-table-head>
                ${page.items.map(
                  (row) => html`
                    <uui-table-row>
                      <uui-table-cell>${row.id}</uui-table-cell>
                      <uui-table-cell>${row.name}</uui-table-cell>
                      <uui-table-cell>${row.totalSubscribers ?? "—"}</uui-table-cell>
                    </uui-table-row>
                  `,
                )}
              </uui-table>
            `
          : html`<p class="empty">Ingen lister fundet.</p>`}
      </uui-box>
    `;
  }

  #renderSegments(page) {
    return html`
      <uui-box headline="Segmenter">
        <p class="intro">${page ? `${page.count} segmenter i Brevo` : "Ingen data"}</p>
        ${page?.items?.length
          ? html`
              <uui-table>
                <uui-table-head>
                  <uui-table-head-cell>ID</uui-table-head-cell>
                  <uui-table-head-cell>Navn</uui-table-head-cell>
                  <uui-table-head-cell>Kategori</uui-table-head-cell>
                </uui-table-head>
                ${page.items.map(
                  (row) => html`
                    <uui-table-row>
                      <uui-table-cell>${row.id}</uui-table-cell>
                      <uui-table-cell>${row.name}</uui-table-cell>
                      <uui-table-cell>${row.categoryName ?? "—"}</uui-table-cell>
                    </uui-table-row>
                  `,
                )}
              </uui-table>
            `
          : html`<p class="empty">Ingen segmenter fundet.</p>`}
      </uui-box>
    `;
  }

  #renderCampaigns(page) {
    return html`
      <uui-box headline="E-mailkampagner">
        <p class="intro">${page ? `${page.count} kampagner i Brevo` : "Ingen data"}</p>
        ${page?.items?.length
          ? html`
              <uui-table>
                <uui-table-head>
                  <uui-table-head-cell>Navn</uui-table-head-cell>
                  <uui-table-head-cell>Emne</uui-table-head-cell>
                  <uui-table-head-cell>Status</uui-table-head-cell>
                  <uui-table-head-cell>Oprettet</uui-table-head-cell>
                  <uui-table-head-cell>Sendt</uui-table-head-cell>
                </uui-table-head>
                ${page.items.map(
                  (row) => html`
                    <uui-table-row>
                      <uui-table-cell>${row.name}</uui-table-cell>
                      <uui-table-cell>${row.subject ?? "—"}</uui-table-cell>
                      <uui-table-cell><span class="status">${row.status ?? "—"}</span></uui-table-cell>
                      <uui-table-cell>${this.#formatDate(row.createdAt)}</uui-table-cell>
                      <uui-table-cell>${this.#formatDate(row.sentDate)}</uui-table-cell>
                    </uui-table-row>
                  `,
                )}
              </uui-table>
            `
          : html`<p class="empty">Ingen kampagner fundet.</p>`}
      </uui-box>
    `;
  }

  #renderContactTable(items) {
    if (!items?.length) {
      return html`<p class="empty">Ingen kontakter fundet.</p>`;
    }

    return html`
      <uui-table>
        <uui-table-head>
          <uui-table-head-cell>E-mail</uui-table-head-cell>
          <uui-table-head-cell>Navn</uui-table-head-cell>
        </uui-table-head>
        ${items.map(
          (row) => html`
            <uui-table-row>
              <uui-table-cell>${row.email}</uui-table-cell>
              <uui-table-cell>${this.#contactName(row)}</uui-table-cell>
            </uui-table-row>
          `,
        )}
      </uui-table>
    `;
  }

  static styles = [
    UmbTextStyles,
    css`
      :host {
        display: block;
        height: 100%;
      }
      .layout {
        height: 100%;
        box-sizing: border-box;
      }
      .header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        width: 100%;
        padding-inline: var(--uui-size-layout-1);
        box-sizing: border-box;
      }
      uui-tab-group {
        flex: 1;
      }
      .content {
        padding: var(--uui-size-layout-1);
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
        box-sizing: border-box;
      }
      uui-box {
        margin: 0;
      }
      .intro {
        margin: 0 0 0.75rem;
        color: var(--uui-color-text-alt);
      }
      .meta-list {
        margin: 0;
        padding-left: 1.25rem;
      }
      .ok {
        color: var(--uui-color-positive);
      }
      .bad {
        color: var(--uui-color-danger);
      }
      .warn-box {
        border-left: 3px solid var(--uui-color-warning);
      }
      .empty {
        margin: 0;
        color: var(--uui-color-text-alt);
      }
      .status {
        text-transform: capitalize;
      }
      uui-table {
        width: 100%;
      }
    `,
  ];
}

customElements.define("tnc-brevo-dashboard", TncBrevoDashboardElement);

export default TncBrevoDashboardElement;
