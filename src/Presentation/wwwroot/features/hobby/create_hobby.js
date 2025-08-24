import { api } from '/features/utils/api.js';

function wireCreateHobby(){
  const addBtn = document.getElementById('addHobby');
  if(!addBtn) return;
  addBtn.addEventListener('click', async ()=>{
    const input = document.getElementById('hobbyName');
    const name = input.value.trim();
    const editId = input.dataset.editId;
    if(!name) return;
    if(editId){
      await api(`/api/v1/hobbies/${editId}`, { method:'PUT', body: JSON.stringify({ Name: name }) });
      delete input.dataset.editId;
    } else {
      await api('/api/v1/hobbies', { method:'POST', body: JSON.stringify({ Name: name }) });
    }
    input.value='';
    // reload list
    const evt = new Event('click'); document.getElementById('hobbiesSearch').dispatchEvent(evt);
  });
}

document.addEventListener('DOMContentLoaded', wireCreateHobby);
