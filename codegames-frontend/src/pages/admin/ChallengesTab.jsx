import { useEffect, useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import api from '../../api/client.js'
import { challengeTypeLabel } from '../../components/challenge/challengeTypes.js'
import { configTemplates, usesConfig, usesOptions } from './challengeTemplates.js'
import { useAdminUi } from './AdminUi.jsx'

const types = [
  'PredictOutput',
  'MultipleChoice',
  'FixBug',
  'SwipeBug',
  'FillBlanks',
  'ReorderCode',
  'TraceExecution',
  'RobotMaze',
]

const emptyChallenge = {
  type: 'PredictOutput',
  language: 'Python',
  difficulty: 1,
  title: '',
  points: 10,
  explanation: '',
  codeSnippet: '',
  question: '',
  correctAnswer: '',
  configJson: '',
  isActive: true,
  isInDailyPool: false,
  options: [
    { text: '', isCorrect: true },
    { text: '', isCorrect: false },
  ],
}

export default function ChallengesTab() {
  const { toast, confirm } = useAdminUi()
  const [typeFilter, setTypeFilter] = useState('')
  const [page, setPage] = useState(1)
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [editing, setEditing] = useState(null)
  const [jsonError, setJsonError] = useState('')
  const [serverError, setServerError] = useState('')
  const [pool, setPool] = useState(null)

  const form = useForm({ defaultValues: emptyChallenge })
  const options = useFieldArray({ control: form.control, name: 'options' })
  const selectedType = form.watch('type')

  async function load(nextPage = page, nextType = typeFilter) {
    setLoading(true)
    try {
      const [list, daily] = await Promise.all([
        api.get('/admin/challenges', {
          params: { page: nextPage, pageSize: 8, type: nextType || undefined },
        }),
        api.get('/admin/daily'),
      ])
      setResult(list.data)
      setPool(daily.data)
    } catch (error) {
      toast(error.message, 'bad')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load(page, typeFilter)
  }, [page, typeFilter])

  function openCreate() {
    setEditing('new')
    setServerError('')
    setJsonError('')
    form.reset(emptyChallenge)
  }

  async function openEdit(id) {
    setServerError('')
    setJsonError('')
    try {
      const response = await api.get(`/admin/challenges/${id}`)
      const challenge = response.data
      setEditing(challenge.id)
      form.reset({
        type: challenge.type,
        language: challenge.language,
        difficulty: challenge.difficulty,
        title: challenge.title,
        points: challenge.points,
        explanation: challenge.explanation,
        codeSnippet: challenge.codeSnippet ?? '',
        question: challenge.question ?? '',
        correctAnswer: challenge.correctAnswer ?? '',
        configJson: challenge.configJson ?? '',
        isActive: challenge.isActive,
        isInDailyPool: challenge.isInDailyPool,
        options: challenge.options?.length
          ? challenge.options.map((option) => ({ text: option.text, isCorrect: option.isCorrect }))
          : emptyChallenge.options,
      })
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  function onTypeChange(event) {
    const next = event.target.value
    form.setValue('type', next)
    const currentJson = form.getValues('configJson')?.trim()
    if (configTemplates[next] && !currentJson) {
      form.setValue('configJson', configTemplates[next])
    }
  }

  function insertExample() {
    const type = form.getValues('type')
    if (configTemplates[type]) {
      form.setValue('configJson', configTemplates[type])
      setJsonError('')
    }
  }

  function formatJson() {
    const raw = form.getValues('configJson')
    try {
      form.setValue('configJson', JSON.stringify(JSON.parse(raw), null, 2))
      setJsonError('')
    } catch {
      setJsonError('JSON nije ispravan.')
    }
  }

  function markCorrect(index) {
    const current = form.getValues('options')
    form.setValue('options', current.map((option, optionIndex) => ({
      ...option,
      isCorrect: optionIndex === index,
    })))
  }

  async function onSave(values) {
    setServerError('')
    const payload = {
      type: values.type,
      language: values.language,
      difficulty: Number(values.difficulty),
      title: values.title,
      points: Number(values.points),
      explanation: values.explanation,
      isActive: Boolean(values.isActive),
      isInDailyPool: Boolean(values.isInDailyPool),
      codeSnippet: values.codeSnippet,
      question: values.question,
      correctAnswer: values.correctAnswer,
      configJson: usesConfig(values.type) ? values.configJson : null,
      options: usesOptions(values.type)
        ? values.options.map((option) => ({ text: option.text, isCorrect: Boolean(option.isCorrect) }))
        : [],
    }

    try {
      if (editing === 'new') {
        await api.post('/admin/challenges', payload)
        toast('Zadatak je dodan.')
      } else {
        await api.put(`/admin/challenges/${editing}`, payload)
        toast('Zadatak je spremljen.')
      }

      setEditing(null)
      await load(page, typeFilter)
    } catch (error) {
      setServerError(error.message)
    }
  }

  async function onDeactivate(item) {
    const accepted = await confirm({
      title: 'Deaktivirati zadatak?',
      message: `„${item.title}” više se neće nuditi igračima. Pokušaji ostaju.`,
      confirmLabel: 'Deaktiviraj',
    })
    if (!accepted) {
      return
    }

    try {
      const response = await api.post(`/admin/challenges/${item.id}/deactivate`)
      toast(response.data.message)
      await load(page, typeFilter)
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  async function onDelete(item) {
    const accepted = await confirm({
      title: 'Obrisati zadatak?',
      message: `„${item.title}” se briše samo ako nema pokušaja. Inače ga deaktiviraj.`,
      confirmLabel: 'Obriši',
    })
    if (!accepted) {
      return
    }

    try {
      const response = await api.delete(`/admin/challenges/${item.id}`)
      toast(response.data.message)
      await load(page, typeFilter)
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  const items = result?.items ?? []
  const pages = result ? Math.max(1, Math.ceil(result.total / result.pageSize)) : 1
  const optionMode = usesOptions(selectedType)
  const configMode = usesConfig(selectedType)

  return (
    <div>
      <div className="admin-toolbar">
        <label>
          Tip
          <select
            aria-label="Filter po tipu"
            value={typeFilter}
            onChange={(event) => {
              setPage(1)
              setTypeFilter(event.target.value)
            }}
          >
            <option value="">Svi tipovi</option>
            {types.map((type) => (
              <option key={type} value={type}>{challengeTypeLabel(type)}</option>
            ))}
          </select>
        </label>
        <button type="button" className="button" onClick={openCreate}>Novi zadatak</button>
      </div>

      {pool ? (
        <div className={pool.poolCount < pool.poolTarget ? 'feedback feedback-bad' : 'feedback feedback-ok'}>
          <p>Bazen: {pool.poolCount}/{pool.poolTarget}</p>
          {pool.message ? <p>{pool.message}</p> : null}
          {pool.selection?.length > 0 ? (
            <p>
              Današnji odabir: {pool.selection.map((slot) => slot.title).join(', ')}
            </p>
          ) : null}
        </div>
      ) : null}

      {loading ? <p role="status">Učitavanje...</p> : null}
      {!loading && items.length === 0 ? <p className="empty">Nema zadataka za ovaj filter.</p> : null}

      <div className="admin-list">
        {items.map((item) => (
          <div className="admin-row admin-challenge" key={item.id}>
            <strong>{item.title}</strong>
            <span>{challengeTypeLabel(item.type)}</span>
            <span>{item.language} · težina {item.difficulty} · {item.points}</span>
            <span className={item.isActive ? 'attempt-ok' : 'attempt-bad'}>
              {item.isActive ? 'Aktivan' : 'Neaktivan'}
              {item.isInDailyPool ? ' · bazen' : ''}
            </span>
            <span className="admin-actions">
              <button type="button" onClick={() => openEdit(item.id)}>Uredi</button>
              <button type="button" onClick={() => onDeactivate(item)} disabled={!item.isActive}>Deaktiviraj</button>
              <button type="button" onClick={() => onDelete(item)}>Obriši</button>
            </span>
          </div>
        ))}
      </div>

      <div className="admin-pager">
        <button type="button" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>Prethodna</button>
        <span>Stranica {page} / {pages}</span>
        <button type="button" disabled={page >= pages} onClick={() => setPage((value) => value + 1)}>Sljedeća</button>
      </div>

      {editing ? (
        <form className="form panel" onSubmit={form.handleSubmit(onSave)} noValidate>
          <h2>{editing === 'new' ? 'Novi zadatak' : 'Uredi zadatak'}</h2>
          <label>
            Tip
            <select {...form.register('type')} onChange={onTypeChange}>
              {types.map((type) => (
                <option key={type} value={type}>{challengeTypeLabel(type)}</option>
              ))}
            </select>
          </label>
          <label>
            Naslov
            <input {...form.register('title', { required: 'Naslov je obavezan.' })} />
            {form.formState.errors.title ? <span className="error">{form.formState.errors.title.message}</span> : null}
          </label>
          <label>
            Jezik
            <input {...form.register('language', { required: 'Jezik je obavezan.' })} />
          </label>
          <label>
            Težina
            <select {...form.register('difficulty', { valueAsNumber: true })}>
              <option value={1}>1</option>
              <option value={2}>2</option>
              <option value={3}>3</option>
            </select>
          </label>
          <label>
            Bodovi
            <input type="number" min="1" max="100" {...form.register('points', {
              required: 'Bodovi su obavezni.',
              valueAsNumber: true,
              min: { value: 1, message: 'Najmanje 1.' },
              max: { value: 100, message: 'Najviše 100.' },
            })}
            />
            {form.formState.errors.points ? <span className="error">{form.formState.errors.points.message}</span> : null}
          </label>
          <label>
            Objašnjenje
            <textarea rows={3} {...form.register('explanation', { required: 'Objašnjenje je obavezno.' })} />
            {form.formState.errors.explanation ? <span className="error">{form.formState.errors.explanation.message}</span> : null}
          </label>
          <label className="check-row">
            <input type="checkbox" {...form.register('isActive')} />
            Aktivan
          </label>
          <label className="check-row">
            <input type="checkbox" {...form.register('isInDailyPool')} />
            U bazenu dnevnih izazova
          </label>

          {!configMode ? (
            <>
              <label>
                Isječak koda
                <textarea rows={5} {...form.register('codeSnippet', {
                  required: selectedType === 'MultipleChoice' ? false : 'Isječak koda je obavezan.',
                })}
                />
              </label>
              <label>
                Pitanje
                <textarea rows={2} {...form.register('question', { required: 'Pitanje je obavezno.' })} />
                {form.formState.errors.question ? <span className="error">{form.formState.errors.question.message}</span> : null}
              </label>
            </>
          ) : null}

          {selectedType === 'PredictOutput' ? (
            <label>
              Točan ispis
              <textarea rows={2} {...form.register('correctAnswer', { required: 'Točan ispis je obavezan.' })} />
              {form.formState.errors.correctAnswer ? <span className="error">{form.formState.errors.correctAnswer.message}</span> : null}
            </label>
          ) : null}

          {optionMode ? (
            <fieldset className="options">
              <legend>Opcije</legend>
              {options.fields.map((field, index) => (
                <div className="option-edit" key={field.id}>
                  <label className="check-row">
                    <input
                      type="radio"
                      name="correct-option"
                      checked={Boolean(form.watch(`options.${index}.isCorrect`))}
                      onChange={() => markCorrect(index)}
                    />
                    Točna
                  </label>
                  <input
                    aria-label={`Opcija ${index + 1}`}
                    {...form.register(`options.${index}.text`, { required: 'Tekst opcije je obavezan.' })}
                  />
                  <button type="button" onClick={() => options.remove(index)} disabled={options.fields.length <= 2}>
                    Ukloni
                  </button>
                </div>
              ))}
              <button type="button" onClick={() => options.append({ text: '', isCorrect: false })}>Dodaj opciju</button>
            </fieldset>
          ) : null}

          {configMode ? (
            <label>
              ConfigJson
              <textarea className="config-json" rows={14} {...form.register('configJson', { required: 'ConfigJson je obavezan.' })} />
              <span className="admin-actions">
                <button type="button" onClick={insertExample}>Umetni primjer</button>
                <button type="button" onClick={formatJson}>Format JSON</button>
              </span>
              {jsonError ? <span className="error">{jsonError}</span> : null}
              {form.formState.errors.configJson ? <span className="error">{form.formState.errors.configJson.message}</span> : null}
            </label>
          ) : null}

          {serverError ? <p className="error" role="alert">{serverError}</p> : null}
          <div className="admin-actions">
            <button type="submit" className="button" disabled={form.formState.isSubmitting}>Spremi</button>
            <button type="button" onClick={() => setEditing(null)}>Odustani</button>
          </div>
        </form>
      ) : null}
    </div>
  )
}
