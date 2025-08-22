import { api } from '/feature/utils/api.js';
import { refreshList, toList } from '/feature/freelancer/state_freelancer.js';
import { resetForm } from '/ui/ui.js';

// Handles create/update form submit and reset
export function wireCreateForm(){
  const form = document.getElementById('freelancerForm');
  if(!form) return;
  form.addEventListener('submit', async (e)=>{
    e.preventDefault();
    const id = document.getElementById('freelancerId').value;
    const body = JSON.stringify({
      username: document.getElementById('username').value.trim(),
      email: document.getElementById('email').value.trim(),
      phoneNumber: document.getElementById('phone').value.trim(),
      skillsets: toList(document.getElementById('skillsets').value||''),
      hobbies: toList(document.getElementById('hobbies').value||'')
    });
    if(id) await api(`/api/v1/freelancers/${id}`, { method:'PUT', body });
    else await api(`/api/v1/freelancers`, { method:'POST', body });
    resetForm();
    refreshList();
  });
  document.getElementById('resetBtn')?.addEventListener('click', resetForm);
}

// auto-wire on DOM ready
document.addEventListener('DOMContentLoaded', wireCreateForm);
