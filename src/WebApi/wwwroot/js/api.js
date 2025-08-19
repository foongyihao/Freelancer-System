/**
 * API helper module: network + error display.
 */
(function(global){
	/**
	 * Show an error banner (expects #error + #errorMsg in DOM). Safe if absent.
	 * @param {string} message
	 */
	function showError(message){
		const box = document.getElementById('error');
		const span = document.getElementById('errorMsg');
		if(!box || !span) return;
		span.textContent = message;
		box.style.display = 'block';
	}

	/** Hide and clear error banner. */
	function clearError(){
		const box = document.getElementById('error');
		const span = document.getElementById('errorMsg');
		if(!box || !span) return;
		box.style.display = 'none';
		span.textContent='';
	}

	/**
	 * Generic fetch wrapper with single body read + JSON parse + ProblemDetails handling.
	 * @param {string} path
	 * @param {RequestInit} [opts]
	 * @returns {Promise<any|null>}
	 */
	async function api(path, opts={}){
		try {
			const resp = await fetch(path, { headers:{'Content-Type':'application/json'}, ...opts });
			const ctype = resp.headers.get('content-type')||'';
			const raw = await resp.text();
			if(!resp.ok){
				let msg = '';
				if(raw){
					if(ctype.includes('application/json')){
						try { const d = JSON.parse(raw); msg = d.title || d.message || d.error || d.detail || JSON.stringify(d);} catch { msg = raw; }
					} else msg = raw;
				}
				throw new Error(msg || `Request failed (${resp.status})`);
			}
			if(!raw) return null;
			if(ctype.includes('application/json')){ try { return JSON.parse(raw); } catch { return null; } }
			return raw;
		} catch(err){
			showError(err instanceof Error ? err.message : String(err));
			throw err;
		}
	}

	// Expose
	global.AppApi = { api, showError, clearError };
})(window);
