/** Entry point wiring event handlers. */
(function(global){
	const { AppState } = global;
	const { api, clearError } = global.AppApi;
	const { loadAll } = global.AppUI;

	function search(){
		AppState.currentSearch = document.getElementById('searchTerm').value.trim();
		AppState.currentPage = 1;
		loadAll();
	}
	global.search = search;

	function resetForm(){
		const form = document.getElementById('freelancerForm');
		form.reset();
		document.getElementById('freelancerId').value='';
	}
	global.resetForm = resetForm;

	async function submitForm(e){
		e.preventDefault();
		const form = document.getElementById('freelancerForm');
		const id = document.getElementById('freelancerId').value;
		const payload = {
			username: username.value.trim(),
			email: email.value.trim(),
			phoneNumber: phone.value.trim(),
			skillsets: toList(skillsets.value),
			hobbies: toList(hobbies.value)
		};
		clearError();
		if(id) await api(`/api/v1/freelancers/${id}`, { method:'PUT', body: JSON.stringify(payload) }).catch(()=>{});
		else await api(`/api/v1/freelancers`, { method:'POST', body: JSON.stringify(payload) }).catch(()=>{});
		resetForm();
		if(AppState.currentSearch) search(); else loadAll();
	}

	function updatePagerButtons(){
		document.getElementById('prevBtn').addEventListener('click', ()=> { if(AppState.currentPage>1){ AppState.currentPage--; loadAll(); }});
		document.getElementById('nextBtn').addEventListener('click', ()=> { if(AppState.currentPage<AppState.totalPages){ AppState.currentPage++; loadAll(); }});
	}

	function wireEvents(){
		document.getElementById('freelancerForm').addEventListener('submit', submitForm);
		document.getElementById('resetBtn').addEventListener('click', resetForm);
		document.getElementById('searchBtn').addEventListener('click', search);
		document.getElementById('refreshBtn').addEventListener('click', ()=> { AppState.currentSearch=''; AppState.currentPage=1; loadAll(); });
		document.getElementById('showArchived').addEventListener('change', ()=> { AppState.currentPage=1; AppState.currentSearch=''; loadAll(); });
		updatePagerButtons();
	}

	document.addEventListener('DOMContentLoaded', ()=> { wireEvents(); loadAll(); });
})(window);
