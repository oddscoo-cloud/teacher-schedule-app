import React, { useEffect, useState } from "react";
import { createNote, deleteNote, fetchNotes, updateNote } from "../../api";

const emptyNote = { content: "", goals: "", progress: "" };

const StudentNotesList = ({ studentId, canEdit = true }) => {
  const [notes, setNotes] = useState([]);
  const [draft, setDraft] = useState(emptyNote);
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");

  const loadNotes = async () => {
    try {
      setNotes(await fetchNotes(studentId));
      setError("");
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  useEffect(() => {
    loadNotes();
    // Load once per student.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [studentId]);

  const saveNote = async (event) => {
    event.preventDefault();
    try {
      if (editingId) {
        await updateNote(studentId, editingId, draft);
      } else {
        await createNote(studentId, draft);
      }
      setDraft(emptyNote);
      setEditingId(null);
      await loadNotes();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  const editNote = (note) => {
    setEditingId(note.id);
    setDraft({
      content: note.content,
      goals: note.goals || "",
      progress: note.progress || "",
    });
  };

  const removeNote = async (noteId) => {
    try {
      await deleteNote(studentId, noteId);
      await loadNotes();
    } catch (requestError) {
      setError(requestError.message);
    }
  };

  return (
    <section className="student-notes">
      <h2>Личные заметки</h2>
      {error && <p role="alert" className="student-error">{error}</p>}
      {notes.length === 0 && !error && <p>Заметок пока нет.</p>}
      <ul>
        {notes.map((note) => (
          <li key={note.id} className="student-note">
            <p>{note.content}</p>
            {note.goals && <p><strong>Цели:</strong> {note.goals}</p>}
            {note.progress && <p><strong>Прогресс:</strong> {note.progress}</p>}
            <small>{new Date(note.createdAt).toLocaleDateString()}</small>
            {canEdit && (
              <div>
                <button type="button" onClick={() => editNote(note)}>Изменить</button>
                <button type="button" onClick={() => removeNote(note.id)}>Удалить</button>
              </div>
            )}
          </li>
        ))}
      </ul>
      {canEdit && (
        <form onSubmit={saveNote} className="student-note-form">
          <label>
            Заметка *
            <textarea
              required
              value={draft.content}
              onChange={(event) => setDraft({ ...draft, content: event.target.value })}
            />
          </label>
          <label>
            Цели
            <textarea
              value={draft.goals}
              onChange={(event) => setDraft({ ...draft, goals: event.target.value })}
            />
          </label>
          <label>
            Прогресс
            <textarea
              value={draft.progress}
              onChange={(event) => setDraft({ ...draft, progress: event.target.value })}
            />
          </label>
          <button type="submit">{editingId ? "Обновить заметку" : "Добавить заметку"}</button>
          {editingId && (
            <button
              type="button"
              onClick={() => {
                setDraft(emptyNote);
                setEditingId(null);
              }}
            >
              Отмена
            </button>
          )}
        </form>
      )}
    </section>
  );
};

export default StudentNotesList;
