import { api } from '/features/utils/api.js';
import { refreshList, toList } from '/features/freelancer/state_freelancer.js';
import { resetForm } from '/ui/ui.js';

// Handles create/update form submit and reset
export function wireCreateForm(){
  const form = document.getElementById('freelancerForm');
  if(!form) return;
  form.addEventListener('submit', async (e)=>{
    e.preventDefault();
    const id = document.getElementById('freelancerId').value;
    // Collect selected ids from the hidden inputs (populated by navigate_freelancer via selects)
    const skillsetIds = toList(document.getElementById('skillsets').value||'').filter(x=>x);
    const hobbyIds = toList(document.getElementById('hobbies').value||'').filter(x=>x);
    const body = JSON.stringify({
      username: document.getElementById('username').value.trim(),
      email: document.getElementById('email').value.trim(),
      phoneNumber: document.getElementById('phone').value.trim(),
      skillsetIds,
      hobbyIds
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
