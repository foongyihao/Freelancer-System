import { api } from '/feature/utils/api.js';

function wireCreateSkill(){
  const addBtn = document.getElementById('addSkill');
  if(!addBtn) return;
  addBtn.addEventListener('click', async ()=>{
    const input = document.getElementById('skillName');
    const name = input.value.trim();
    const editId = input.dataset.editId;
    if(!name) return;
    if(editId){
      await api(`/api/v1/skills/${editId}`, { method:'PUT', body: JSON.stringify({ Name: name }) });
      delete input.dataset.editId;
    } else {
      await api('/api/v1/skills', { method:'POST', body: JSON.stringify({ Name: name }) });
    }
    input.value='';
    import('/feature/skillset/navigate_skillset.js').then(m=>m.default ? m.default() : 0);
    // reload list
    const evt = new Event('click'); document.getElementById('skillsSearch').dispatchEvent(evt);
  });
}

document.addEventListener('DOMContentLoaded', wireCreateSkill);
