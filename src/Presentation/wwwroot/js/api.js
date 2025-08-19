/**
 * Generic fetch helper that performs a request, safely reads the body once and
 * returns parsed JSON (or raw text) on success. Non-2xx responses are converted
 * into thrown Errors with a human-friendly message.
 * @param {string} path - Absolute or relative request URL.
 * @param {RequestInit} [opts] - Additional fetch options (method, body, headers, etc.).
 * @returns {Promise<any|null>} Parsed JSON object, plain text string, or null when no body.
 */
import { showError } from './ui.js';

export async function api(path, opts={}) {
  try {
    const resp = await fetch(path, { headers: { 'Content-Type':'application/json' }, ...opts });
    const contentType = resp.headers.get('content-type') || '';
    const raw = await resp.text();
    if(!resp.ok){
      let msg = '';
      if(raw){
        if(contentType.includes('application/json')){
          try {
            const data = JSON.parse(raw);
            msg = data.title || data.message || data.error || data.detail || JSON.stringify(data);
          } catch { msg = raw; }
        } else {
          msg = raw;
        }
      }
      throw new Error(msg || `Request failed (${resp.status})`);
    }
    if(!raw) return null;
    if(contentType.includes('application/json')){
      try { return JSON.parse(raw); } catch { return null; }
    }
    return raw;
  } catch(err){
    showError(err instanceof Error ? err.message : String(err));
    throw err;
  }
}
export default api;
