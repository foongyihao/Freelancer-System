// UI helpers used across frontend modules
export function showError(message){
	const box = document.getElementById('error');
	const span = document.getElementById('errorMsg');
	if(!box || !span) return;
	box.style.display = 'block';
	span.textContent = message;
}

export function clearError(){
	const box = document.getElementById('error');
	if(!box) return;
	box.style.display = 'none';
	const span = document.getElementById('errorMsg');
	if(span) span.textContent='';
}

export function renderFreelancerCard(items, onAction){
	const list = document.getElementById('list');
	if(!list) return;
	list.innerHTML='';
	(items||[]).forEach(f=> {
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
		card.querySelectorAll('button').forEach(btn=> btn.addEventListener('click', ()=> onAction && onAction(f, btn.dataset.act)));
		list.appendChild(card);
	});
}

export function updatePager(currentPage, totalPages){
	const pageInfo = document.getElementById('pageInfo');
	const pager = document.getElementById('pager');
	const prev = document.getElementById('prevBtn');
	const next = document.getElementById('nextBtn');
	if(pageInfo) pageInfo.textContent = `Page ${currentPage} / ${totalPages}`;
	if(pager) pager.style.display = 'flex';
	if(prev) prev.disabled = currentPage<=1;
	if(next) next.disabled = currentPage>=totalPages;
}

export function resetForm(){
	const form = document.getElementById('freelancerForm');
	if(form) form.reset();
	const idEl = document.getElementById('freelancerId');
	if(idEl) idEl.value='';
}
