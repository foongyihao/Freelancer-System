/** UI rendering functions. Depends on AppState & AppApi. */
(function(global){
	const { AppState } = global;
	const { api, clearError } = global.AppApi;

	/** Render freelancer list cards. */
	function render(items){
		const list = document.getElementById('list');
		if(!list) return;
		list.innerHTML='';
		(items||[]).forEach(f=>{
			const card = document.createElement('div');
			card.className='card'+(f.isArchived?' archived':'');
			card.innerHTML = `
				<strong>${f.username}</strong> <span class="arch" style="display:${f.isArchived?'inline':'none'}">ARCHIVED</span><br>
				<small>${f.email}</small><br>
				<small>${f.phoneNumber||''}</small><br>
				<div class="skills">${(f.skillsets||[]).map(s=>`<span class='badge'>${s.name}</span>`).join('')}</div>
				<div class="hobbies">${(f.hobbies||[]).map(h=>`<span class='badge'>${h.name}</span>`).join('')}</div>
				<div style='margin-top:.5rem;'>
					<button data-act='edit'>Edit</button>
					<button data-act='archive'>${f.isArchived?'Unarchive':'Archive'}</button>
					<button data-act='delete'>Delete</button>
				</div>`;
			card.querySelectorAll('button').forEach(btn=> btn.addEventListener('click', ()=> handleAction(f, btn.dataset.act)));
			list.appendChild(card);
		});
	}

	/** Update pagination controls. */
	function updatePager(){
		const info = document.getElementById('pageInfo');
		if(!info) return;
		info.textContent = `Page ${AppState.currentPage} / ${AppState.totalPages}`;
		const prevBtn = document.getElementById('prevBtn');
		const nextBtn = document.getElementById('nextBtn');
		if(prevBtn) prevBtn.disabled = AppState.currentPage<=1;
		if(nextBtn) nextBtn.disabled = AppState.currentPage>=AppState.totalPages;
	}

	/** Load (list or search) freelancers. */
	async function loadAll(){
		const includeArchived = document.getElementById('showArchived').checked;
		clearError();
		const termParam = AppState.currentSearch?`&term=${encodeURIComponent(AppState.currentSearch)}`:'';
		const data = await api(`/api/v1/freelancers?includeArchived=${includeArchived}&page=${AppState.currentPage}&pageSize=${AppState.pageSize}${termParam}`).catch(()=>null);
		const items = data?.items || [];
		AppState.totalPages = data?.totalPages || 1;
		render(items);
		updatePager();
	}

	/** Handle card actions. */
	async function handleAction(f, act){
		if(act==='edit'){
			document.getElementById('freelancerId').value=f.id;
			username.value=f.username; email.value=f.email; phone.value=f.phoneNumber;
			skillsets.value=(f.skillsets||[]).map(s=>s.name).join(',');
			hobbies.value=(f.hobbies||[]).map(h=>h.name).join(',');
			window.scrollTo({top:0, behavior:'smooth'});
		} else if(act==='archive'){
			await fetch(`/api/v1/freelancers/${f.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ isArchived: !f.isArchived }) });
			if(AppState.currentSearch){ search(); } else { loadAll(); }
		} else if(act==='delete'){
			if(confirm('Delete?')){
				await fetch(`/api/v1/freelancers/${f.id}`, { method:'DELETE' });
				if(!AppState.currentSearch){
					const includeArchived = document.getElementById('showArchived').checked;
					const pageCheck = await api(`/api/v1/freelancers?includeArchived=${includeArchived}&page=${AppState.currentPage}&pageSize=${AppState.pageSize}`);
					if(pageCheck.items && pageCheck.items.length === 0 && AppState.currentPage>1){ AppState.currentPage--; }
					loadAll();
				} else {
					search();
				}
			}
		}
	}

	// Export to global
	global.AppUI = { render, updatePager, loadAll, handleAction };
})(window);
