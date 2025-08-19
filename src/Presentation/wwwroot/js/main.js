/**
 * Handle card action button clicks.
 * @param {Object} f - Freelancer entity.
 * @param {string} act - Action identifier ('edit' | 'archive' | 'delete').
 * @returns {Promise<void>}
 */
import { fetchFreelancers, refreshList, search, toList, currentPage, pageSize, currentSearch, currentSkillFilter, currentHobbyFilter } from './state.js';
import { api } from './api.js';
import { resetForm, clearError } from './ui.js';

export async function handleAction(f, act){
  if(act==='edit'){
    document.getElementById('freelancerId').value=f.id;
    username.value=f.username; email.value=f.email; phone.value=f.phoneNumber;
    skillsets.value=(f.skillsets||[]).map(s=>s.name).join(',');
    hobbies.value=(f.hobbies||[]).map(h=>h.name).join(',');
    window.scrollTo({top:0, behavior:'smooth'});
  } else if(act==='archive'){
    await fetch(`/api/v1/freelancers/${f.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ isArchived: !f.isArchived }) });
  refreshList();
  } else if(act==='delete'){
    if(confirm('Delete?')){ 
      await fetch(`/api/v1/freelancers/${f.id}`, { method:'DELETE' });
  // Adjust page if we deleted last item on page
  const includeArchived = document.getElementById('showArchived').checked;
  const pageCheck = await api(`/api/v1/freelancers?includeArchived=${includeArchived}&page=${currentPage}&pageSize=${pageSize}`);
  if(pageCheck.items && pageCheck.items.length === 0 && currentPage>1){ currentPage--; }
  refreshList();
    }
  }
}

// Initialize event listeners when the DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
  // Form submit
  document.getElementById('freelancerForm').addEventListener('submit', async (e)=>{
    e.preventDefault();
    const id = document.getElementById('freelancerId').value;
    const payload = {
      username: document.getElementById('username').value.trim(),
      email: document.getElementById('email').value.trim(),
      phoneNumber: document.getElementById('phone').value.trim(),
      skillsets: toList(document.getElementById('skillsets').value),
      hobbies: toList(document.getElementById('hobbies').value)
    };
    if(id){
      clearError();
      await api(`/api/v1/freelancers/${id}`, { method:'PUT', body: JSON.stringify(payload) }).catch(()=>{});
    } else {
      clearError();
      await api(`/api/v1/freelancers`, { method:'POST', body: JSON.stringify(payload) }).catch(()=>{});
    }
    resetForm();
  refreshList();
  });

  // Button event listeners
  document.getElementById('resetBtn').addEventListener('click', resetForm);
  document.getElementById('searchBtn').addEventListener('click', search);
  document.getElementById('refreshBtn').addEventListener('click', ()=> { 
    currentSearch=''; currentSkillFilter=''; currentHobbyFilter='';
    document.getElementById('searchTerm').value='';
    const sk=document.getElementById('searchSkill'); if(sk) sk.value='';
    const hb=document.getElementById('searchHobby'); if(hb) hb.value='';
    currentPage=1; refreshList(); 
  });
  document.getElementById('showArchived').addEventListener('change', ()=> { currentPage=1; currentSearch=''; refreshList(); });
  document.getElementById('prevBtn').addEventListener('click', ()=> { if(currentPage>1){ currentPage--; refreshList(); }});
  document.getElementById('nextBtn').addEventListener('click', ()=> { if(currentPage<totalPages){ currentPage++; refreshList(); }});
  document.getElementById('dismissError').addEventListener('click', clearError);

  // Load initial data
  refreshList();
});
