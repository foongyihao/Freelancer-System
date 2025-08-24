import { api } from '/features/utils/api.js';
import { renderFreelancerCard, updatePager } from '/ui/ui.js';

export let currentPage = 1;
export let totalPages = 1;
export const pageSize = 10;
export let currentSearch = '';
export let currentSkillFilter = '';
export let currentHobbyFilter = '';

export function gotoPage(p){ currentPage = p; }
export function decPage(){ if(currentPage>1) currentPage--; }
export function incPage(){ if(currentPage<totalPages) currentPage++; }
export function resetSearchFilters(){ currentSearch=''; currentSkillFilter=''; currentHobbyFilter=''; }
export function setTotalPages(t){ totalPages = t; }

export async function fetchFreelancers({ term, skill, hobby } = {}) {
  if (term !== undefined) currentSearch = term.trim();
  if (skill !== undefined) currentSkillFilter = skill.trim();
  if (hobby !== undefined) currentHobbyFilter = hobby.trim();
  const includeArchived = document.getElementById('showArchived').checked;

  const q = new URLSearchParams();
  q.set('includeArchived', includeArchived);
  q.set('page', currentPage);
  q.set('pageSize', pageSize);
  if (currentSearch) q.set('term', currentSearch);
  if (currentSkillFilter) q.set('skill', currentSkillFilter);
  if (currentHobbyFilter) q.set('hobby', currentHobbyFilter);

  const data = await api(`/api/v1/freelancers?${q.toString()}`).catch(()=>null);
  const items = data?.items || [];
  setTotalPages(data?.totalPages || 1);
  renderFreelancerCard(items, (f, act) => import('/features/freelancer/navigate_freelancer.js').then(m=> (m.handleAction || m.handleFreelancerAction)(f, act)));
  updatePager(currentPage, totalPages);
}

export async function search() {
  currentPage = 1;
  const skillEl = document.getElementById('searchSkill');
  const hobbyEl = document.getElementById('searchHobby');
  const skill = skillEl && skillEl.multiple ? Array.from(skillEl.selectedOptions).map(o=>o.value).filter(v=>v).join(',') : (skillEl?.value || '');
  const hobby = hobbyEl && hobbyEl.multiple ? Array.from(hobbyEl.selectedOptions).map(o=>o.value).filter(v=>v).join(',') : (hobbyEl?.value || '');
  return fetchFreelancers({
    term: document.getElementById('searchTerm').value,
    skill,
    hobby
  });
}

export function refreshList(){ return fetchFreelancers(); }

export function toList(v){ return v.split(',').map(x=>x.trim()).filter(Boolean); }
