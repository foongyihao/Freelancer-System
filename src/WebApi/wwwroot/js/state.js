/** Application state (pagination + search). */
window.AppState = {
	currentPage: 1,
	totalPages: 1,
	pageSize: 10,
	currentSearch: ''
};

/** Split comma separated input into trimmed tokens. */
function toList(v){ return (v||'').split(',').map(x=>x.trim()).filter(Boolean); }
window.toList = toList;
