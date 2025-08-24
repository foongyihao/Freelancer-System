import { api } from '/features/utils/api.js';
import { refreshList, currentPage, pageSize } from '/features/freelancer/state_freelancer.js';
import { resetForm, clearError } from '/ui/ui.js';

// Load options into a dropdown
async function loadSelectOptions(selectId, url, opts={}){
  const select = document.getElementById(selectId);
  if(!select) return;
  const data = await api(url).catch(()=>null);
  const items = data?.items || [];
  select.innerHTML='';
  const placeholder = document.createElement('option');
  placeholder.value = '';
  placeholder.disabled = true;
  placeholder.selected = true;
  placeholder.textContent = opts.placeholder ?? 'Select... [hold CMD for multiple]';
  select.appendChild(placeholder);
  items.forEach(x=>{
    const opt = document.createElement('option');
    // Use id as value but show name to user
    opt.value = x.id || x.name; // fallback if older payload
    opt.textContent = x.name;
    select.appendChild(opt);
  });
}

/**
 * Handle card actions for freelancers.
 */
export async function handleFreelancerAction(f, act){
  if(act==='edit'){
    document.getElementById('freelancerId').value=f.id;
    username.value=f.username; email.value=f.email; phone.value=f.phoneNumber;
  // Store ids into hidden inputs for submission
  skillsets.value=(f.skillsets||[]).map(s=>s.id||s.name).join(',');
  hobbies.value=(f.hobbies||[]).map(h=>h.id||h.name).join(',');
    window.scrollTo({top:0, behavior:'smooth'});
  } else if(act==='archive'){
    await fetch(`/api/v1/freelancers/${f.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ isArchived: !f.isArchived }) });
    refreshList();
  } else if(act==='delete'){
    if(confirm('Delete?')){
      await fetch(`/api/v1/freelancers/${f.id}`, { method:'DELETE' });
      const includeArchived = document.getElementById('showArchived').checked;
      const pageCheck = await api(`/api/v1/freelancers?includeArchived=${includeArchived}&page=${currentPage}&pageSize=${pageSize}`);
      if(pageCheck.items && pageCheck.items.length === 0 && currentPage>1){ currentPage--; }
      refreshList();
    }
  }
}

// Event wiring for navigation/search/paging (form submit handled in create_freelancer.js)
document.addEventListener('DOMContentLoaded', function() {
  const withState = (fn) => import('/features/freelancer/state_freelancer.js').then(m=>fn(m));
  document.getElementById('searchBtn')?.addEventListener('click', ()=> withState(m=>m.search()));
  document.getElementById('refreshBtn')?.addEventListener('click', ()=> withState(m=>{ m.resetSearchFilters(); ['searchTerm','searchSkill','searchHobby'].forEach(id=>{ const el=document.getElementById(id); if(el) el.value='';}); m.gotoPage(1); m.refreshList(); }));
  document.getElementById('showArchived')?.addEventListener('change', ()=> withState(m=>{ m.resetSearchFilters(); m.gotoPage(1); m.refreshList(); }));
  document.getElementById('prevBtn')?.addEventListener('click', ()=> withState(m=>{ if(m.currentPage>1){ m.decPage(); m.refreshList(); }}));
  document.getElementById('nextBtn')?.addEventListener('click', ()=> withState(m=>{ m.incPage(); m.refreshList(); }));
  document.getElementById('dismissError')?.addEventListener('click', clearError);

  // Picker: skills multi-select -> merge into #skillsets
  const skillsSelect = document.getElementById('skillsSelect');
  if(skillsSelect){
  skillsSelect.addEventListener('change', ()=>{
  const picked = Array.from(skillsSelect.selectedOptions).map(o=>o.value).filter(v=>v);
      const input = document.getElementById('skillsets');
      if(!input) return;
  const typed = (input.value||'').split(',').map(x=>x.trim()).filter(Boolean);
    const merged = Array.from(new Set([...typed, ...picked]));
      input.value = merged.join(',');
    });
    // Initial load of skills
    loadSelectOptions('skillsSelect', '/api/v1/skills?page=1&pageSize=50');
  }

  const hobbiesSelect = document.getElementById('hobbiesSelect');
  if(hobbiesSelect){
  hobbiesSelect.addEventListener('change', ()=>{
  const picked = Array.from(hobbiesSelect.selectedOptions).map(o=>o.value).filter(v=>v);
      const input = document.getElementById('hobbies');
      if(!input) return;
  const typed = (input.value||'').split(',').map(x=>x.trim()).filter(Boolean);
    const merged = Array.from(new Set([...typed, ...picked]));
      input.value = merged.join(',');
    });
    // Initial load of hobbies
    loadSelectOptions('hobbiesSelect', '/api/v1/hobbies?page=1&pageSize=50');
  }

  loadSelectOptions('searchSkill', '/api/v1/skills?page=1&pageSize=200');
  loadSelectOptions('searchHobby', '/api/v1/hobbies?page=1&pageSize=200');
  // Initial load
  refreshList();
});
