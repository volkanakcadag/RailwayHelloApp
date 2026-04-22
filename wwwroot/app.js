const state = {
    selectedId: null,
    records: []
};

const recordForm = document.getElementById("recordForm");
const recordIdInput = document.getElementById("recordId");
const titleInput = document.getElementById("title");
const categoryInput = document.getElementById("category");
const quantityInput = document.getElementById("quantity");
const unitPriceInput = document.getElementById("unitPrice");
const isActiveInput = document.getElementById("isActive");
const tableBody = document.getElementById("recordsTableBody");
const connectionStatus = document.getElementById("connectionStatus");
const formStatus = document.getElementById("formStatus");
const recordCount = document.getElementById("recordCount");

document.getElementById("testConnectionButton").addEventListener("click", testConnection);
document.getElementById("refreshButton").addEventListener("click", loadRecords);
document.getElementById("newButton").addEventListener("click", resetForm);
document.getElementById("deleteButton").addEventListener("click", deleteRecord);
recordForm.addEventListener("submit", saveRecord);

initialize();

async function initialize() {
    await testConnection();
    await loadRecords();
}

async function testConnection() {
    setStatus(connectionStatus, "Baglanti kontrol ediliyor...", "neutral");

    try {
        const response = await fetch("/api/db-status");
        const payload = await readJson(response);

        if (!response.ok) {
            throw new Error(payload?.detail ?? payload?.message ?? "Baglanti kurulamadi.");
        }

        const timeText = payload.serverTimeUtc ? ` UTC: ${payload.serverTimeUtc}` : "";
        setStatus(connectionStatus, `${payload.message}${timeText}`, "success");
    } catch (error) {
        setStatus(connectionStatus, error.message, "error");
    }
}

async function loadRecords() {
    try {
        const response = await fetch("/api/test-records");
        const payload = await readJson(response);

        if (!response.ok) {
            throw new Error(payload?.detail ?? payload?.message ?? "Kayitlar alinamadi.");
        }

        state.records = payload;
        renderTable();
        setStatus(formStatus, "Kayit listesi yenilendi.", "neutral");
    } catch (error) {
        state.records = [];
        renderTable();
        setStatus(formStatus, error.message, "error");
    }
}

async function saveRecord(event) {
    event.preventDefault();

    const body = {
        title: titleInput.value.trim(),
        category: categoryInput.value.trim(),
        quantity: Number(quantityInput.value),
        unitPrice: Number(unitPriceInput.value),
        isActive: isActiveInput.checked
    };

    const isUpdate = Boolean(state.selectedId);
    const url = isUpdate ? `/api/test-records/${state.selectedId}` : "/api/test-records";
    const method = isUpdate ? "PUT" : "POST";

    setStatus(formStatus, "Kayit veritabanina yaziliyor...", "neutral");

    try {
        const response = await fetch(url, {
            method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        });

        const payload = await readJson(response);
        if (!response.ok) {
            throw new Error(payload?.message ?? payload?.detail ?? "Kayit islenemedi.");
        }

        setStatus(
            formStatus,
            isUpdate
                ? `Kayit guncellendi ve transaction commit edildi. ID: ${payload.id}`
                : `Yeni kayit eklendi ve transaction commit edildi. ID: ${payload.id}`,
            "success");

        await loadRecords();
        selectRecord(payload.id);
    } catch (error) {
        setStatus(formStatus, error.message, "error");
    }
}

async function deleteRecord() {
    if (!state.selectedId) {
        setStatus(formStatus, "Silmek icin once bir kayit sec.", "error");
        return;
    }

    try {
        const response = await fetch(`/api/test-records/${state.selectedId}`, {
            method: "DELETE"
        });

        if (!response.ok && response.status !== 404) {
            const payload = await readJson(response);
            throw new Error(payload?.message ?? payload?.detail ?? "Kayit silinemedi.");
        }

        setStatus(formStatus, "Kayit silindi ve transaction commit edildi.", "success");
        resetForm();
        await loadRecords();
    } catch (error) {
        setStatus(formStatus, error.message, "error");
    }
}

function renderTable() {
    recordCount.textContent = `${state.records.length} kayit`;

    if (state.records.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="7" class="empty-row">Kayit bulunamadi.</td></tr>`;
        return;
    }

    tableBody.innerHTML = state.records.map((record) => `
        <tr data-id="${record.id}" class="${record.id === state.selectedId ? "selected" : ""}">
            <td>${record.id}</td>
            <td>${escapeHtml(record.title)}</td>
            <td>${escapeHtml(record.category)}</td>
            <td>${record.quantity}</td>
            <td>${Number(record.unitPrice).toFixed(2)}</td>
            <td>${record.isActive ? "Evet" : "Hayir"}</td>
            <td>${formatDate(record.createdAt)}</td>
        </tr>
    `).join("");

    tableBody.querySelectorAll("tr[data-id]").forEach((row) => {
        row.addEventListener("click", () => {
            selectRecord(Number(row.dataset.id));
        });
    });
}

function selectRecord(id) {
    const record = state.records.find((item) => item.id === id);
    if (!record) {
        return;
    }

    state.selectedId = record.id;
    recordIdInput.value = record.id;
    titleInput.value = record.title;
    categoryInput.value = record.category;
    quantityInput.value = record.quantity;
    unitPriceInput.value = record.unitPrice;
    isActiveInput.checked = record.isActive;

    renderTable();
    setStatus(formStatus, `ID ${record.id} secildi. Degisiklik yapip Kaydet'e basabilirsin.`, "neutral");
}

function resetForm() {
    state.selectedId = null;
    recordForm.reset();
    recordIdInput.value = "";
    quantityInput.value = 0;
    unitPriceInput.value = 0;
    isActiveInput.checked = true;
    renderTable();
    setStatus(formStatus, "Yeni kayit ekleme modundasin.", "neutral");
}

function setStatus(element, message, type) {
    element.textContent = message;
    element.className = `status-card ${type}`;
}

async function readJson(response) {
    const contentType = response.headers.get("content-type") ?? "";
    if (!contentType.includes("json")) {
        return null;
    }

    return response.json();
}

function formatDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat("tr-TR", {
        dateStyle: "short",
        timeStyle: "short"
    }).format(date);
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}
