import { api } from '/features/utils/api.js';

let skillsPage = 1;
let skillsTotal = 1;
const pageSize = 12;
let skillsTerm = '';

function renderSkills(items){
  const grid = document.getElementById('skillsGrid');
  if(!grid) return;
  grid.innerHTML='';
  (items||[]).forEach(s=>{
    const card = document.createElement('div');
    card.className='card';
    card.innerHTML = `
      <strong>${s.name}</strong>
      <div style='margin-top:.5rem;'>
        <button data-act='edit'>Edit</button>
        <button data-act='delete'>Delete</button>
      </div>`;
    card.querySelectorAll('button').forEach(btn=> btn.addEventListener('click', ()=> handleSkillAction(s, btn.dataset.act)));
    grid.appendChild(card);
  });
  document.getElementById('skillsPageInfo').textContent = `Page ${skillsPage} / ${skillsTotal}`;
  document.getElementById('skillsPrev').disabled = skillsPage<=1;
  document.getElementById('skillsNext').disabled = skillsPage>=skillsTotal;
}

async function loadSkills(){
  const q = new URLSearchParams();
  q.set('page', skillsPage); q.set('pageSize', pageSize);
  if(skillsTerm) q.set('term', skillsTerm);
  const data = await api(`/api/v1/skills?${q.toString()}`).catch(()=>null);
  skillsTotal = data?.totalPages || 1;
  renderSkills(data?.items||[]);
}

export function handleSkillAction(s, act){
  if(act==='edit'){
    document.getElementById('skillName').value = s.name;
    document.getElementById('skillName').dataset.editId = s.id;
  } else if(act==='delete'){
    if(confirm('Delete skill?')){
      api(`/api/v1/skills/${s.id}`, { method:'DELETE' }).then(loadSkills);
    }
  }
}

function wireSkillNav(){
  document.getElementById('skillsSearch')?.addEventListener('click', ()=>{ skillsPage=1; skillsTerm = document.getElementById('skillsTerm').value.trim(); loadSkills(); });
  document.getElementById('skillsPrev')?.addEventListener('click', ()=>{ if(skillsPage>1){ skillsPage--; loadSkills(); }});
  document.getElementById('skillsNext')?.addEventListener('click', ()=>{ skillsPage++; loadSkills(); });
}

document.addEventListener('DOMContentLoaded', ()=>{ wireSkillNav(); loadSkills(); });
